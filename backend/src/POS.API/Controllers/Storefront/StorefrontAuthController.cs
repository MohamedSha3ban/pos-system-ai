using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Storefront.DTOs;
using POS.Application.Modules.Storefront.Services;

namespace POS.API.Controllers.Storefront;

/// <summary>Customer account creation/login for a specific tenant's storefront -- public, no bearer token needed.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/storefront/{tenantId:guid}/auth")]
public class StorefrontAuthController : ControllerBase
{
    private readonly CustomerAuthService _customerAuthService;
    public StorefrontAuthController(CustomerAuthService customerAuthService) => _customerAuthService = customerAuthService;

    [HttpPost("register")]
    public async Task<ActionResult<CustomerAuthResponse>> Register(Guid tenantId, CustomerRegisterRequest request, CancellationToken ct)
    {
        var result = await _customerAuthService.RegisterAsync(tenantId, request, ct);
        return result is null ? Conflict(new { error = "An account with this email already exists." }) : Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<CustomerAuthResponse>> Login(Guid tenantId, CustomerLoginRequest request, CancellationToken ct)
    {
        var result = await _customerAuthService.LoginAsync(tenantId, request, ct);
        return result is null ? Unauthorized() : Ok(result);
    }
}
