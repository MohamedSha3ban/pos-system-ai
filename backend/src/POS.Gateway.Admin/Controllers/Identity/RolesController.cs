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
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;
    public RolesController(IMediator mediator) => _mediator = mediator;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpGet]
    [RequirePermission(Permissions.RolesManage)]
    public async Task<ActionResult<List<RoleDto>>> GetAll(CancellationToken ct)
        => Ok(await _mediator.Send(new GetRolesQuery(TenantId), ct));

    [HttpPost]
    [RequirePermission(Permissions.RolesManage)]
    public async Task<ActionResult<RoleDto>> Create(UpsertRoleRequest request, CancellationToken ct)
        => Ok(await _mediator.Send(new CreateRoleCommand(TenantId, request), ct));

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.RolesManage)]
    public async Task<ActionResult<RoleDto>> Update(Guid id, UpsertRoleRequest request, CancellationToken ct)
    {
        var updated = await _mediator.Send(new UpdateRoleCommand(TenantId, id, request), ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.RolesManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var (success, error) = await _mediator.Send(new DeleteRoleCommand(TenantId, id), ct);
        return success ? NoContent() : BadRequest(new { error });
    }
}
