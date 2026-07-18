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
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    public UsersController(UserService userService) => _userService = userService;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpGet]
    [RequirePermission(Permissions.UsersManage)]
    public async Task<ActionResult<List<UserDto>>> GetAll(CancellationToken ct)
        => Ok(await _userService.GetAllAsync(TenantId, ct));

    [HttpPost]
    [RequirePermission(Permissions.UsersManage)]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken ct)
        => Ok(await _userService.CreateAsync(TenantId, request, ct));

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.UsersManage)]
    public async Task<ActionResult<UserDto>> Update(Guid id, UpdateUserRequest request, CancellationToken ct)
    {
        var updated = await _userService.UpdateAsync(TenantId, id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Deactivates rather than hard-deletes (see UserService).</summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.UsersManage)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var ok = await _userService.DeactivateAsync(TenantId, id, ct);
        return ok ? NoContent() : NotFound();
    }
}
