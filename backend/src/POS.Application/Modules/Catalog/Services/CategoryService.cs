using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Catalog.DTOs;
using POS.Domain.Modules.Catalog.Entities;

namespace POS.Application.Modules.Catalog.Services;

public class CategoryService
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext _readDb;

    public CategoryService(IWriteDbContext writeDb, IReadDbContext readDb)
    {
        _writeDb = writeDb;
        _readDb = readDb;
    }

    /// <summary>Independent list read -- read side.</summary>
    public async Task<List<CategoryDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await _readDb.Categories
            .Where(c => c.TenantId == tenantId && !c.IsDeleted)
            .Select(c => new CategoryDto(c.Id, c.Name, _readDb.Products.Count(p => p.CategoryId == c.Id && !p.IsDeleted)))
            .ToListAsync(ct);
    }

    public async Task<CategoryDto> CreateAsync(Guid tenantId, UpsertCategoryRequest request, CancellationToken ct = default)
    {
        var category = new Category { TenantId = tenantId, Name = request.Name };
        _writeDb.Categories.Add(category);
        await _writeDb.SaveChangesAsync(ct);
        return new CategoryDto(category.Id, category.Name, 0);
    }

    public async Task<CategoryDto?> UpdateAsync(Guid tenantId, Guid categoryId, UpsertCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _writeDb.Categories.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == categoryId && !c.IsDeleted, ct);
        if (category is null) return null;

        category.Name = request.Name;
        category.UpdatedAtUtc = DateTime.UtcNow;
        await _writeDb.SaveChangesAsync(ct);

        var productCount = await _writeDb.Products.CountAsync(p => p.CategoryId == categoryId && !p.IsDeleted, ct);
        return new CategoryDto(category.Id, category.Name, productCount);
    }

    public async Task<bool> DeleteAsync(Guid tenantId, Guid categoryId, CancellationToken ct = default)
    {
        var category = await _writeDb.Categories.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Id == categoryId && !c.IsDeleted, ct);
        if (category is null) return false;

        category.IsDeleted = true;
        category.UpdatedAtUtc = DateTime.UtcNow;
        await _writeDb.SaveChangesAsync(ct);
        return true;
    }
}
