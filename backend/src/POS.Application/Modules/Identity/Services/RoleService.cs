using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Identity.DTOs;
using POS.Domain.Common;
using POS.Domain.Modules.Identity.Entities;

namespace POS.Application.Modules.Identity.Services;

public class RoleService
{
    private readonly IApplicationDbContext _db;
    public RoleService(IApplicationDbContext db) => _db = db;

    public async Task<List<RoleDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        var roles = await _db.Roles.Where(r => r.TenantId == tenantId && !r.IsDeleted).ToListAsync(ct);
        return roles.Select(ToDto).ToList();
    }

    public List<string> GetAvailablePermissions() => Permissions.TenantAssignable.ToList();

    public async Task<RoleDto> CreateAsync(Guid tenantId, UpsertRoleRequest request, CancellationToken ct = default)
    {
        var role = new Role
        {
            TenantId = tenantId,
            Name = request.Name,
            IsSystemRole = false,
            PermissionsCsv = string.Join(',', SanitizePermissions(request.Permissions))
        };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync(ct);
        return ToDto(role);
    }

    public async Task<RoleDto?> UpdateAsync(Guid tenantId, Guid roleId, UpsertRoleRequest request, CancellationToken ct = default)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == roleId && !r.IsDeleted, ct);
        if (role is null) return null;

        // System role names are protected (Owner/Manager/Cashier), but their permission
        // sets can still be tuned by the tenant.
        if (!role.IsSystemRole) role.Name = request.Name;
        role.PermissionsCsv = string.Join(',', SanitizePermissions(request.Permissions));
        role.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToDto(role);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid tenantId, Guid roleId, CancellationToken ct = default)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == roleId && !r.IsDeleted, ct);
        if (role is null) return (false, "Role not found.");
        if (role.IsSystemRole) return (false, "System roles (Owner/Manager/Cashier) can't be deleted.");

        var inUse = await _db.UserRoleAssignments.AnyAsync(a => a.RoleId == roleId, ct);
        if (inUse) return (false, "Role is still assigned to one or more users.");

        role.IsDeleted = true;
        role.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return (true, null);
    }

    private static IEnumerable<string> SanitizePermissions(List<string> requested) =>
        requested.Where(p => Permissions.TenantAssignable.Contains(p)).Distinct();

    private static RoleDto ToDto(Role r) => new(
        r.Id, r.Name, r.IsSystemRole,
        r.PermissionsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
}
