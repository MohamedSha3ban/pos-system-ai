using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Storefront.Commands;
using POS.Application.Modules.Storefront.DTOs;
using POS.Application.Modules.Storefront.Queries;

namespace POS.Gateway.Ecommerce.Controllers.Storefront;

/// <summary>Customer account creation/login/session management for a specific tenant's storefront.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/storefront/{tenantId:guid}/auth")]
public class StorefrontAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public StorefrontAuthController(IMediator mediator) => _mediator = mediator;

    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? ClientUserAgent => Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;

    [HttpPost("register")]
    public async Task<ActionResult<CustomerAuthResponse>> Register(Guid tenantId, CustomerRegisterRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CustomerRegisterCommand(tenantId, request, ClientIp, ClientUserAgent), ct);
        return result is null ? Conflict(new { error = "An account with this email already exists." }) : Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<CustomerAuthResponse>> Login(Guid tenantId, CustomerLoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CustomerLoginQuery(tenantId, request, ClientIp, ClientUserAgent), ct);
        return result is null ? Unauthorized() : Ok(result);
    }

    /// <summary>Silent refresh, same reasoning as the staff gateways' /auth/refresh -- keeps
    /// a shopper signed in without forcing re-login every 30 minutes.</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<CustomerAuthResponse>> Refresh(Guid tenantId, CustomerRefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CustomerRefreshTokenCommand(request.RefreshToken, ClientIp, ClientUserAgent), ct);
        return result is null ? Unauthorized(new { error = "Invalid or expired refresh token." }) : Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(Guid tenantId, CustomerRefreshTokenRequest request, CancellationToken ct)
    {
        await _mediator.Send(new CustomerLogoutCommand(request.RefreshToken), ct);
        return NoContent();
    }
}
