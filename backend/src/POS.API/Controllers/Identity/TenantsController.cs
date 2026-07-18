using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Authorization;
using POS.Application.Modules.Identity.DTOs;
using POS.Application.Modules.Identity.Services;

namespace POS.API.Controllers.Identity;

/// <summary>
/// Platform-level tenant management -- only reachable by a User with IsPlatformAdmin=true.
/// This is intentionally NOT tenant-scoped like the rest of the API; it's the "operate the
/// whole SaaS" surface, not a per-business feature.
/// </summary>
[ApiController]
[Authorize]
[RequirePlatformAdmin]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly TenantService _tenantService;
    public TenantsController(TenantService tenantService) => _tenantService = tenantService;

    [HttpGet]
    public async Task<ActionResult<List<TenantSummaryDto>>> GetAll(CancellationToken ct)
        => Ok(await _tenantService.GetAllAsync(ct));

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
        => await _tenantService.SetActiveAsync(id, true, ct) ? NoContent() : NotFound();

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        => await _tenantService.SetActiveAsync(id, false, ct) ? NoContent() : NotFound();
}
