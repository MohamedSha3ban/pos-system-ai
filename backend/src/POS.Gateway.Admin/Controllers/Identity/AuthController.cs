using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Identity.Commands;
using POS.Application.Modules.Identity.DTOs;
using POS.Application.Modules.Identity.Queries;

namespace POS.Gateway.Admin.Controllers.Identity;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? ClientUserAgent => Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;

    /// <summary>Onboards a new business (creates Tenant + owner User + default Location) and starts their first session.</summary>
    [HttpPost("register-tenant")]
    public async Task<ActionResult<AuthResponse>> RegisterTenant(RegisterTenantRequest request, CancellationToken ct)
        => Ok(await _mediator.Send(new RegisterTenantCommand(request, ClientIp, ClientUserAgent), ct));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginQuery(request, ClientIp, ClientUserAgent), ct);
        return result is null ? Unauthorized() : Ok(result);
    }

    /// <summary>
    /// Exchanges a still-valid refresh token for a new access+refresh token pair (rotation).
    /// No [Authorize] -- possession of a valid refresh token IS the authentication for this
    /// endpoint, same as the standard OAuth2 refresh-token grant. The access token doesn't
    /// need to still be valid to call this, which is the whole point: it lets a client stay
    /// signed in silently after the short-lived access token expires.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken, ClientIp, ClientUserAgent), ct);
        return result is null ? Unauthorized(new { error = "Invalid or expired refresh token." }) : Ok(result);
    }

    /// <summary>Revokes exactly the session tied to the presented refresh token. Idempotent, no [Authorize] needed.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken ct)
    {
        await _mediator.Send(new LogoutCommand(request.RefreshToken), ct);
        return NoContent();
    }

    /// <summary>"Log out everywhere" -- requires a currently-valid access token, since it operates on "my" sessions.</summary>
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll(CancellationToken ct)
    {
        var tenantId = Guid.Parse(User.FindFirstValue("tenantId")!);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        await _mediator.Send(new LogoutAllCommand(tenantId, userId), ct);
        return NoContent();
    }

    /// <summary>Lists this user's active (non-revoked, non-expired) sessions. Pass your own
    /// refresh token as ?currentRefreshToken= to have it flagged IsCurrent in the response.</summary>
    [HttpGet("sessions")]
    [Authorize]
    public async Task<ActionResult<List<SessionDto>>> GetSessions([FromQuery] string? currentRefreshToken, CancellationToken ct)
    {
        var tenantId = Guid.Parse(User.FindFirstValue("tenantId")!);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        return Ok(await _mediator.Send(new GetActiveSessionsQuery(tenantId, userId, currentRefreshToken), ct));
    }
}
