using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Identity.DTOs;
using POS.Domain.Common;
using POS.Domain.Modules.Identity.Entities;

namespace POS.Application.Modules.Identity.Services;

public class RoleService
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext _readDb;

    public RoleService(IWriteDbContext writeDb, IReadDbContext readDb)
    {
        _writeDb = writeDb;
        _readDb = readDb;
    }

    /// <summary>Independent list read -- read side.</summary>
    public async Task<List<RoleDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        var roles = await _readDb.Roles.Where(r => r.TenantId == tenantId && !r.IsDeleted).ToListAsync(ct);
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
        _writeDb.Roles.Add(role);
        await _writeDb.SaveChangesAsync(ct);
        return ToDto(role);
    }

    public async Task<RoleDto?> UpdateAsync(Guid tenantId, Guid roleId, UpsertRoleRequest request, CancellationToken ct = default)
    {
        var role = await _writeDb.Roles.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == roleId && !r.IsDeleted, ct);
        if (role is null) return null;

        // System role names are protected (Owner/Manager/Cashier), but their permission
        // sets can still be tuned by the tenant.
        if (!role.IsSystemRole) role.Name = request.Name;
        role.PermissionsCsv = string.Join(',', SanitizePermissions(request.Permissions));
        role.UpdatedAtUtc = DateTime.UtcNow;

        await _writeDb.SaveChangesAsync(ct);
        return ToDto(role);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid tenantId, Guid roleId, CancellationToken ct = default)
    {
        var role = await _writeDb.Roles.FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == roleId && !r.IsDeleted, ct);
        if (role is null) return (false, "Role not found.");
        if (role.IsSystemRole) return (false, "System roles (Owner/Manager/Cashier) can't be deleted.");

        // In-use check goes through the write context, not the read side: this guard and
        // the delete that follows must see the same, current state (a role assigned to a
        // user microseconds ago on the primary shouldn't be deletable because a replica
        // read hasn't caught up yet).
        var inUse = await _writeDb.UserRoleAssignments.AnyAsync(a => a.RoleId == roleId, ct);
        if (inUse) return (false, "Role is still assigned to one or more users.");

        role.IsDeleted = true;
        role.UpdatedAtUtc = DateTime.UtcNow;
        await _writeDb.SaveChangesAsync(ct);
        return (true, null);
    }

    private static IEnumerable<string> SanitizePermissions(List<string> requested) =>
        requested.Where(p => Permissions.TenantAssignable.Contains(p)).Distinct();

    private static RoleDto ToDto(Role r) => new(
        r.Id, r.Name, r.IsSystemRole,
        r.PermissionsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
}
