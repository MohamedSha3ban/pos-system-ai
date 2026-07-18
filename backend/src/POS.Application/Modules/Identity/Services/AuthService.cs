using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Identity.DTOs;
using POS.Application.Modules.Identity.Interfaces;
using POS.Domain.Common;
using POS.Domain.Modules.Identity.Entities;

namespace POS.Application.Modules.Identity.Services;

public class AuthService
{
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(IApplicationDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterTenantAsync(RegisterTenantRequest request, CancellationToken ct = default)
    {
        var tenant = new Tenant
        {
            BusinessName = request.BusinessName,
            BusinessType = request.BusinessType
        };
        _db.Tenants.Add(tenant);

        var defaultLocation = new Location
        {
            TenantId = tenant.Id,
            Name = "Main Location"
        };
        _db.Locations.Add(defaultLocation);

        // Seed the three default roles every tenant starts with. Owner/Manager/Cashier
        // are IsSystemRole=true (can be edited but not deleted) -- tenants can add their
        // own custom roles alongside these from the Roles & Permissions screen.
        var ownerRole = new Role { TenantId = tenant.Id, Name = "Owner", IsSystemRole = true, PermissionsCsv = string.Join(',', Permissions.TenantAssignable) };
        var managerRole = new Role
        {
            TenantId = tenant.Id,
            Name = "Manager",
            IsSystemRole = true,
            PermissionsCsv = string.Join(',', new[] { Permissions.ProductsManage, Permissions.CategoriesManage, Permissions.InventoryManage, Permissions.OrdersView, Permissions.OrdersCheckout })
        };
        var cashierRole = new Role
        {
            TenantId = tenant.Id,
            Name = "Cashier",
            IsSystemRole = true,
            PermissionsCsv = string.Join(',', new[] { Permissions.OrdersCheckout, Permissions.OrdersView })
        };
        _db.Roles.AddRange(ownerRole, managerRole, cashierRole);

        var owner = new User
        {
            TenantId = tenant.Id,
            FullName = request.OwnerFullName,
            Email = request.OwnerEmail.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword)
        };
        _db.Users.Add(owner);

        _db.UserRoleAssignments.Add(new UserRoleAssignment { TenantId = tenant.Id, UserId = owner.Id, RoleId = ownerRole.Id });

        await _db.SaveChangesAsync(ct);

        var permissions = Permissions.TenantAssignable.ToList();
        var token = _tokenService.GenerateToken(owner, new[] { ownerRole.Name }, permissions);
        return new AuthResponse(token, DateTime.UtcNow.AddHours(8), owner.FullName, tenant.Id, owner.IsPlatformAdmin, new List<string> { ownerRole.Name }, permissions);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted && u.IsActive, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var roleIds = await _db.UserRoleAssignments
            .Where(ura => ura.UserId == user.Id)
            .Select(ura => ura.RoleId)
            .ToListAsync(ct);

        var roles = await _db.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync(ct);
        var roleNames = roles.Select(r => r.Name).ToList();
        var permissions = roles
            .SelectMany(r => r.PermissionsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Distinct()
            .ToList();

        var token = _tokenService.GenerateToken(user, roleNames, permissions);
        return new AuthResponse(token, DateTime.UtcNow.AddHours(8), user.FullName, user.TenantId, user.IsPlatformAdmin, roleNames, permissions);
    }
}
