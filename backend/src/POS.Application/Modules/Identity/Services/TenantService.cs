using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Identity.DTOs;

namespace POS.Application.Modules.Identity.Services;

/// <summary>
/// Platform-level (cross-tenant) tenant management -- backs the admin portal's Tenants
/// screen, which only a User with IsPlatformAdmin=true can reach (see
/// POS.API/Authorization/RequirePlatformAdminAttribute). New tenants are still created
/// through AuthService.RegisterTenantAsync (the normal signup flow); this service is
/// for the platform operator's read/deactivate view across all of them.
/// </summary>
public class TenantService
{
    private readonly IApplicationDbContext _db;
    public TenantService(IApplicationDbContext db) => _db = db;

    public async Task<List<TenantSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var tenants = await _db.Tenants.ToListAsync(ct);
        var userCounts = await _db.Users.Where(u => !u.IsDeleted).GroupBy(u => u.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() }).ToListAsync(ct);
        var productCounts = await _db.Products.Where(p => !p.IsDeleted).GroupBy(p => p.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() }).ToListAsync(ct);

        return tenants.Select(t => new TenantSummaryDto(
            t.Id, t.BusinessName, t.BusinessType, t.IsActive, t.CreatedAtUtc,
            userCounts.FirstOrDefault(u => u.TenantId == t.Id)?.Count ?? 0,
            productCounts.FirstOrDefault(p => p.TenantId == t.Id)?.Count ?? 0
        )).ToList();
    }

    public async Task<bool> SetActiveAsync(Guid tenantId, bool isActive, CancellationToken ct = default)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct);
        if (tenant is null) return false;
        tenant.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
