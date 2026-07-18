using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Authorization;
using POS.Application.Modules.Identity.DTOs;
using POS.Application.Modules.Identity.Services;
using POS.Domain.Common;

namespace POS.API.Controllers.Identity;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly RoleService _roleService;
    public RolesController(RoleService roleService) => _roleService = roleService;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpGet]
    [RequirePermission(Permissions.RolesManage)]
    public async Task<ActionResult<List<RoleDto>>> GetAll(CancellationToken ct)
        => Ok(await _roleService.GetAllAsync(TenantId, ct));

    [HttpPost]
    [RequirePermission(Permissions.RolesManage)]
    public async Task<ActionResult<RoleDto>> Create(UpsertRoleRequest request, CancellationToken ct)
        => Ok(await _roleService.CreateAsync(TenantId, request, ct));

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.RolesManage)]
    public async Task<ActionResult<RoleDto>> Update(Guid id, UpsertRoleRequest request, CancellationToken ct)
    {
        var updated = await _roleService.UpdateAsync(TenantId, id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.RolesManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var (success, error) = await _roleService.DeleteAsync(TenantId, id, ct);
        return success ? NoContent() : BadRequest(new { error });
    }
}
