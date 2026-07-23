using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Identity.Commands;
using POS.Application.Modules.Identity.DTOs;
using POS.Application.Modules.Identity.Queries;
using POS.Gateway.Admin.Authorization;

namespace POS.Gateway.Admin.Controllers.Identity;

/// <summary>
/// Platform-level tenant management -- only reachable by a User with IsPlatformAdmin=true.
/// Intentionally NOT tenant-scoped like the rest of the API; it's the "operate the whole
/// SaaS" surface, not a per-business feature. Exists only in the Admin gateway -- neither
/// the Ecommerce nor Mobile gateway exposes this controller at all.
/// </summary>
[ApiController]
[Authorize]
[RequirePlatformAdmin]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TenantsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<TenantSummaryDto>>> GetAll(CancellationToken ct)
        => Ok(await _mediator.Send(new GetTenantsQuery(), ct));

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
        => await _mediator.Send(new SetTenantActiveCommand(id, true), ct) ? NoContent() : NotFound();

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        => await _mediator.Send(new SetTenantActiveCommand(id, false), ct) ? NoContent() : NotFound();
}
