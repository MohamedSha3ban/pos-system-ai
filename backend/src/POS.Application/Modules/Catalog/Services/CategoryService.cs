using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Catalog.DTOs;
using POS.Domain.Modules.Catalog.Entities;

namespace POS.Application.Modules.Catalog.Services;

public class CategoryService
{
    private readonly IApplicationDbContext _db;
    public CategoryService(IApplicationDbContext db) => _db = db;

    public async Task<List<CategoryDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _db.Categories
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Products.Count(p => !p.IsDeleted)))
            .ToListAsync(ct);
    }

    public async Task<CategoryDto> CreateAsync(Guid tenantId, UpsertCategoryRequest request, CancellationToken ct = default)
    {
        var category = new Category { TenantId = tenantId, Name = request.Name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        return new CategoryDto(category.Id, category.Name, 0);
    }

    public async Task<CategoryDto?> UpdateAsync(Guid tenantId, Guid categoryId, UpsertCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == categoryId && !c.IsDeleted, ct);
        if (category is null) return null;

        category.Name = request.Name;
        category.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return new CategoryDto(category.Id, category.Name, category.Products.Count(p => !p.IsDeleted));
    }

    public async Task<bool> DeleteAsync(Guid tenantId, Guid categoryId, CancellationToken ct = default)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == categoryId && !c.IsDeleted, ct);
        if (category is null) return false;

        category.IsDeleted = true;
        category.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
