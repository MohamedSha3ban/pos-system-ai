using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Identity.DTOs;

namespace POS.Application.Modules.Identity.Services;

/// <summary>
/// Platform-level (cross-tenant) tenant management -- backs the admin portal's Tenants
/// screen, which only a User with IsPlatformAdmin=true can reach.
/// </summary>
public class TenantService
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext _readDb;

    public TenantService(IWriteDbContext writeDb, IReadDbContext readDb)
    {
        _writeDb = writeDb;
        _readDb = readDb;
    }

    /// <summary>Cross-tenant aggregation read -- exactly the kind of heavier, independent
    /// query that benefits most from running against a replica. Read side.</summary>
    public async Task<List<TenantSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tenants = await _readDb.Tenants.ToListAsync(ct);
        var userCounts = await _readDb.Users.Where(u => !u.IsDeleted).GroupBy(u => u.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() }).ToListAsync(ct);
        var productCounts = await _readDb.Products.Where(p => !p.IsDeleted).GroupBy(p => p.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() }).ToListAsync(ct);

        return tenants.Select(t => new TenantSummaryDto(
            t.Id, t.BusinessName, t.BusinessType, t.IsActive, t.CreatedAtUtc,
            userCounts.FirstOrDefault(u => u.TenantId == t.Id)?.Count ?? 0,
            productCounts.FirstOrDefault(p => p.TenantId == t.Id)?.Count ?? 0
        )).ToList();
    }

    public async Task<bool> SetActiveAsync(Guid tenantId, bool isActive, CancellationToken ct = default)
    {
        var tenant = await _writeDb.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct);
        if (tenant is null) return false;
        tenant.IsActive = isActive;
        await _writeDb.SaveChangesAsync(ct);
        return true;
    }
}
