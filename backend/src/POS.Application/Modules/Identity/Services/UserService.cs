using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Identity.DTOs;
using POS.Domain.Modules.Identity.Entities;

namespace POS.Application.Modules.Identity.Services;

public class UserService
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext _readDb;

    public UserService(IWriteDbContext writeDb, IReadDbContext readDb)
    {
        _writeDb = writeDb;
        _readDb = readDb;
    }

    /// <summary>Independent list read -- read side.</summary>
    public async Task<List<UserDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        var users = await _readDb.Users.Where(u => u.TenantId == tenantId && !u.IsDeleted).ToListAsync(ct);
        var assignments = await _readDb.UserRoleAssignments.Where(a => a.TenantId == tenantId).ToListAsync(ct);
        var roles = await _readDb.Roles.Where(r => r.TenantId == tenantId && !r.IsDeleted).ToListAsync(ct);

        return BuildDtos(users, assignments, roles);
    }

    /// <summary>Write + build the response from the just-written entities (no re-query against the read side).</summary>
    public async Task<UserDto> CreateAsync(Guid tenantId, CreateUserRequest request, CancellationToken ct = default)
    {
        var user = new User
        {
            TenantId = tenantId,
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        _writeDb.Users.Add(user);

        var distinctRoleIds = request.RoleIds.Distinct().ToList();
        foreach (var roleId in distinctRoleIds)
            _writeDb.UserRoleAssignments.Add(new UserRoleAssignment { TenantId = tenantId, UserId = user.Id, RoleId = roleId });

        await _writeDb.SaveChangesAsync(ct);

        var roles = await _writeDb.Roles.Where(r => distinctRoleIds.Contains(r.Id)).ToListAsync(ct);
        return new UserDto(user.Id, user.FullName, user.Email, user.IsActive, roles.Select(r => new RoleSummary(r.Id, r.Name)).ToList());
    }

    public async Task<UserDto?> UpdateAsync(Guid tenantId, Guid userId, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _writeDb.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == userId && !u.IsDeleted, ct);
        if (user is null) return null;

        user.FullName = request.FullName;
        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        var existing = await _writeDb.UserRoleAssignments.Where(a => a.UserId == userId).ToListAsync(ct);
        foreach (var toRemove in existing.Where(a => !request.RoleIds.Contains(a.RoleId)))
            _writeDb.UserRoleAssignments.Remove(toRemove);

        foreach (var roleId in request.RoleIds.Where(id => existing.All(a => a.RoleId != id)))
            _writeDb.UserRoleAssignments.Add(new UserRoleAssignment { TenantId = tenantId, UserId = userId, RoleId = roleId });

        await _writeDb.SaveChangesAsync(ct);

        var roles = await _writeDb.Roles.Where(r => request.RoleIds.Contains(r.Id)).ToListAsync(ct);
        return new UserDto(user.Id, user.FullName, user.Email, user.IsActive, roles.Select(r => new RoleSummary(r.Id, r.Name)).ToList());
    }

    /// <summary>Deactivates rather than hard-deletes -- keeps historical order/audit trails intact.</summary>
    public async Task<bool> DeactivateAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var user = await _writeDb.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == userId && !u.IsDeleted, ct);
        if (user is null) return false;

        user.IsActive = false;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _writeDb.SaveChangesAsync(ct);
        return true;
    }

    private static List<UserDto> BuildDtos(List<User> users, List<UserRoleAssignment> assignments, List<Role> roles) =>
        users.Select(u =>
        {
            var roleIds = assignments.Where(a => a.UserId == u.Id).Select(a => a.RoleId).ToHashSet();
            var roleSummaries = roles.Where(r => roleIds.Contains(r.Id)).Select(r => new RoleSummary(r.Id, r.Name)).ToList();
            return new UserDto(u.Id, u.FullName, u.Email, u.IsActive, roleSummaries);
        }).ToList();
}
