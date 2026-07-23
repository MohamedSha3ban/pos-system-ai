using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Catalog.Commands;
using POS.Application.Modules.Catalog.DTOs;
using POS.Application.Modules.Catalog.Queries;
using POS.Domain.Common;
using POS.Gateway.Admin.Authorization;

namespace POS.Gateway.Admin.Controllers.Catalog;

[ApiController]
[Authorize]
[RequirePermission(Permissions.InventoryManage)]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;
    public InventoryController(IMediator mediator) => _mediator = mediator;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpGet]
    public async Task<ActionResult<List<InventoryItemDto>>> GetAll([FromQuery] Guid? locationId, CancellationToken ct)
        => Ok(await _mediator.Send(new GetInventoryQuery(TenantId, locationId), ct));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Adjust(Guid id, AdjustInventoryRequest request, CancellationToken ct)
    {
        var ok = await _mediator.Send(new AdjustInventoryCommand(TenantId, id, request), ct);
        return ok ? NoContent() : NotFound();
    }
}
