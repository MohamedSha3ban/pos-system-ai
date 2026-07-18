using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Identity.DTOs;
using POS.Application.Modules.Identity.Interfaces;
using POS.Domain.Common;
using POS.Domain.Modules.Identity.Entities;

namespace POS.Application.Modules.Identity.Services;

public class AuthService
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext _readDb;
    private readonly ITokenService _tokenService;

    public AuthService(IWriteDbContext writeDb, IReadDbContext readDb, ITokenService tokenService)
    {
        _writeDb = writeDb;
        _readDb = readDb;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Everything here goes through the write context, including the response construction
    /// at the end -- we just created this tenant/roles/user, so we build the AuthResponse
    /// from the in-memory objects rather than re-querying (which, against a real replica,
    /// could momentarily 404 due to replication lag).
    /// </summary>
    public async Task<AuthResponse> RegisterTenantAsync(RegisterTenantRequest request, CancellationToken ct = default)
    {
        var tenant = new Tenant
        {
            BusinessName = request.BusinessName,
            BusinessType = request.BusinessType
        };
        _writeDb.Tenants.Add(tenant);

        var defaultLocation = new Location
        {
            TenantId = tenant.Id,
            Name = "Main Location"
        };
        _writeDb.Locations.Add(defaultLocation);

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
        _writeDb.Roles.AddRange(ownerRole, managerRole, cashierRole);

        var owner = new User
        {
            TenantId = tenant.Id,
            FullName = request.OwnerFullName,
            Email = request.OwnerEmail.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword)
        };
        _writeDb.Users.Add(owner);

        _writeDb.UserRoleAssignments.Add(new UserRoleAssignment { TenantId = tenant.Id, UserId = owner.Id, RoleId = ownerRole.Id });

        await _writeDb.SaveChangesAsync(ct);

        var permissions = Permissions.TenantAssignable.ToList();
        var token = _tokenService.GenerateToken(owner, new[] { ownerRole.Name }, permissions);
        return new AuthResponse(token, DateTime.UtcNow.AddHours(8), owner.FullName, tenant.Id, owner.IsPlatformAdmin, new List<string> { ownerRole.Name }, permissions);
    }

    /// <summary>
    /// Pure read -- uses the read side. Login happening moments after registration and
    /// briefly hitting replication lag is an accepted, standard trade-off (retry succeeds);
    /// nothing here writes.
    /// </summary>
    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await _readDb.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted && u.IsActive, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var roleIds = await _readDb.UserRoleAssignments
            .Where(ura => ura.UserId == user.Id)
            .Select(ura => ura.RoleId)
            .ToListAsync(ct);

        var roles = await _readDb.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync(ct);
        var roleNames = roles.Select(r => r.Name).ToList();
        var permissions = roles
            .SelectMany(r => r.PermissionsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Distinct()
            .ToList();

        var token = _tokenService.GenerateToken(user, roleNames, permissions);
        return new AuthResponse(token, DateTime.UtcNow.AddHours(8), user.FullName, user.TenantId, user.IsPlatformAdmin, roleNames, permissions);
    }
}
