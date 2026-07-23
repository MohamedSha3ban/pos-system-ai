using MediatR;
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

    /// <summary>
    /// Included for completeness/parity with the Admin gateway, though the current
    /// Flutter app only ever calls Login (tenant registration happens on web-admin).
    /// </summary>
    [HttpPost("register-tenant")]
    public async Task<ActionResult<AuthResponse>> RegisterTenant(RegisterTenantRequest request, CancellationToken ct)
        => Ok(await _mediator.Send(new RegisterTenantCommand(request), ct));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginQuery(request), ct);
        return result is null ? Unauthorized() : Ok(result);
    }
}
