using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Identity.DTOs;
using POS.Domain.Modules.Identity.Entities;

namespace POS.Application.Modules.Identity.Services;

public class UserService
{
    private readonly IApplicationDbContext _db;
    public UserService(IApplicationDbContext db) => _db = db;

    public async Task<List<UserDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        var users = await _db.Users.Where(u => u.TenantId == tenantId && !u.IsDeleted).ToListAsync(ct);
        var assignments = await _db.UserRoleAssignments.Where(a => a.TenantId == tenantId).ToListAsync(ct);
        var roles = await _db.Roles.Where(r => r.TenantId == tenantId && !r.IsDeleted).ToListAsync(ct);

        return users.Select(u =>
        {
            var roleIds = assignments.Where(a => a.UserId == u.Id).Select(a => a.RoleId).ToHashSet();
            var roleSummaries = roles.Where(r => roleIds.Contains(r.Id)).Select(r => new RoleSummary(r.Id, r.Name)).ToList();
            return new UserDto(u.Id, u.FullName, u.Email, u.IsActive, roleSummaries);
        }).ToList();
    }

    public async Task<UserDto> CreateAsync(Guid tenantId, CreateUserRequest request, CancellationToken ct = default)
    {
        var user = new User
        {
            TenantId = tenantId,
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        _db.Users.Add(user);

        foreach (var roleId in request.RoleIds.Distinct())
            _db.UserRoleAssignments.Add(new UserRoleAssignment { TenantId = tenantId, UserId = user.Id, RoleId = roleId });

        await _db.SaveChangesAsync(ct);
        return (await GetAllAsync(tenantId, ct)).First(u => u.Id == user.Id);
    }

    public async Task<UserDto?> UpdateAsync(Guid tenantId, Guid userId, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == userId && !u.IsDeleted, ct);
        if (user is null) return null;

        user.FullName = request.FullName;
        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        var existing = await _db.UserRoleAssignments.Where(a => a.UserId == userId).ToListAsync(ct);
        foreach (var toRemove in existing.Where(a => !request.RoleIds.Contains(a.RoleId)))
            _db.UserRoleAssignments.Remove(toRemove);

        foreach (var roleId in request.RoleIds.Where(id => existing.All(a => a.RoleId != id)))
            _db.UserRoleAssignments.Add(new UserRoleAssignment { TenantId = tenantId, UserId = userId, RoleId = roleId });

        await _db.SaveChangesAsync(ct);
        return (await GetAllAsync(tenantId, ct)).FirstOrDefault(u => u.Id == userId);
    }

    /// <summary>Deactivates rather than hard-deletes -- keeps historical order/audit trails intact.</summary>
    public async Task<bool> DeactivateAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == userId && !u.IsDeleted, ct);
        if (user is null) return false;

        user.IsActive = false;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
