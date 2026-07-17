using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Application.Modules.Catalog.DTOs;
using POS.Application.Modules.Catalog.Services;

namespace POS.API.Controllers.Catalog;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;
    public CategoriesController(CategoryService categoryService) => _categoryService = categoryService;

    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenantId")!);

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken ct)
        => Ok(await _categoryService.GetAllAsync(TenantId, ct));

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(UpsertCategoryRequest request, CancellationToken ct)
        => Ok(await _categoryService.CreateAsync(TenantId, request, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, UpsertCategoryRequest request, CancellationToken ct)
    {
        var updated = await _categoryService.UpdateAsync(TenantId, id, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _categoryService.DeleteAsync(TenantId, id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
