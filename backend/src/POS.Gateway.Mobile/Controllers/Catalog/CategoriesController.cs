using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Catalog.Commands;
using POS.Application.Modules.Catalog.DTOs;
using POS.Application.Modules.Catalog.Queries;
using POS.Domain.Common;
using POS.Gateway.Mobile.Authorization;

namespace POS.Gateway.Mobile.Controllers.Catalog;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public CategoriesController(IMediator mediator) => _mediator = mediator;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken ct)
        => Ok(await _mediator.Send(new GetCategoriesQuery(TenantId), ct));

    [HttpPost]
    [RequirePermission(Permissions.CategoriesManage)]
    public async Task<ActionResult<CategoryDto>> Create(UpsertCategoryRequest request, CancellationToken ct)
        => Ok(await _mediator.Send(new CreateCategoryCommand(TenantId, request), ct));

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.CategoriesManage)]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, UpsertCategoryRequest request, CancellationToken ct)
    {
        var updated = await _mediator.Send(new UpdateCategoryCommand(TenantId, id, request), ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.CategoriesManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _mediator.Send(new DeleteCategoryCommand(TenantId, id), ct);
        return deleted ? NoContent() : NotFound();
    }
}
