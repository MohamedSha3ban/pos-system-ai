using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Identity.Commands;
using POS.Application.Modules.Identity.DTOs;
using POS.Application.Modules.Identity.Queries;

namespace POS.Gateway.Mobile.Controllers.Identity;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? ClientUserAgent => Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;

    /// <summary>
    /// Included for completeness/parity with the Admin gateway, though the current
    /// Flutter app only ever calls Login (tenant registration happens on web-admin).
    /// </summary>
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
    /// Silent refresh -- essential for a mobile app, where forcing a full re-login every 15
    /// minutes would be unacceptable UX. No [Authorize]; possession of the refresh token is
    /// the authentication.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken, ClientIp, ClientUserAgent), ct);
        return result is null ? Unauthorized(new { error = "Invalid or expired refresh token." }) : Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken ct)
    {
        await _mediator.Send(new LogoutCommand(request.RefreshToken), ct);
        return NoContent();
    }

    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll(CancellationToken ct)
    {
        var tenantId = Guid.Parse(User.FindFirstValue("tenantId")!);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        await _mediator.Send(new LogoutAllCommand(tenantId, userId), ct);
        return NoContent();
    }

    [HttpGet("sessions")]
    [Authorize]
    public async Task<ActionResult<List<SessionDto>>> GetSessions([FromQuery] string? currentRefreshToken, CancellationToken ct)
    {
        var tenantId = Guid.Parse(User.FindFirstValue("tenantId")!);
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        return Ok(await _mediator.Send(new GetActiveSessionsQuery(tenantId, userId, currentRefreshToken), ct));
    }
}
