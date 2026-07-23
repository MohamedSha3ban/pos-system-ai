using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Identity.Commands;
using POS.Application.Modules.Identity.DTOs;
using POS.Application.Modules.Identity.Queries;
using POS.Domain.Common;
using POS.Gateway.Admin.Authorization;

namespace POS.Gateway.Admin.Controllers.Identity;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    public UsersController(IMediator mediator) => _mediator = mediator;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpGet]
    [RequirePermission(Permissions.UsersManage)]
    public async Task<ActionResult<List<UserDto>>> GetAll(CancellationToken ct)
        => Ok(await _mediator.Send(new GetUsersQuery(TenantId), ct));

    [HttpPost]
    [RequirePermission(Permissions.UsersManage)]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken ct)
        => Ok(await _mediator.Send(new CreateUserCommand(TenantId, request), ct));

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.UsersManage)]
    public async Task<ActionResult<UserDto>> Update(Guid id, UpdateUserRequest request, CancellationToken ct)
    {
        var updated = await _mediator.Send(new UpdateUserCommand(TenantId, id, request), ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.UsersManage)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var ok = await _mediator.Send(new DeactivateUserCommand(TenantId, id), ct);
        return ok ? NoContent() : NotFound();
    }
}
