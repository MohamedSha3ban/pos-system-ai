using Microsoft.AspNetCore.Mvc;
using POS.Application.DTOs.Auth;
using POS.Application.Services;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService) => _authService = authService;

    /// <summary>Onboards a new business (creates Tenant + owner User + default Location).</summary>
    [HttpPost("register-tenant")]
    public async Task<ActionResult<AuthResponse>> RegisterTenant(RegisterTenantRequest request, CancellationToken ct)
        => Ok(await _authService.RegisterTenantAsync(request, ct));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return result is null ? Unauthorized() : Ok(result);
    }
}
