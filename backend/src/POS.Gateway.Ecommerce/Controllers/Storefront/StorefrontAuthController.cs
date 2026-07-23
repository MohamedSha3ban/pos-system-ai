using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Storefront.Commands;
using POS.Application.Modules.Storefront.DTOs;
using POS.Application.Modules.Storefront.Queries;

namespace POS.Gateway.Ecommerce.Controllers.Storefront;

/// <summary>Customer account creation/login for a specific tenant's storefront -- public, no bearer token needed.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/storefront/{tenantId:guid}/auth")]
public class StorefrontAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public StorefrontAuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<ActionResult<CustomerAuthResponse>> Register(Guid tenantId, CustomerRegisterRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CustomerRegisterCommand(tenantId, request), ct);
        return result is null ? Conflict(new { error = "An account with this email already exists." }) : Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<CustomerAuthResponse>> Login(Guid tenantId, CustomerLoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CustomerLoginQuery(tenantId, request), ct);
        return result is null ? Unauthorized() : Ok(result);
    }
}
