using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Authorization;
using POS.Application.Modules.Catalog.DTOs;
using POS.Application.Modules.Catalog.Services;
using POS.Domain.Common;

namespace POS.API.Controllers.Catalog;

[ApiController]
[Authorize]
[RequirePermission(Permissions.InventoryManage)]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;
    public InventoryController(InventoryService inventoryService) => _inventoryService = inventoryService;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpGet]
    public async Task<ActionResult<List<InventoryItemDto>>> GetAll([FromQuery] Guid? locationId, CancellationToken ct)
        => Ok(await _inventoryService.GetAllAsync(TenantId, locationId, ct));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Adjust(Guid id, AdjustInventoryRequest request, CancellationToken ct)
    {
        var ok = await _inventoryService.AdjustAsync(TenantId, id, request, ct);
        return ok ? NoContent() : NotFound();
    }
}
