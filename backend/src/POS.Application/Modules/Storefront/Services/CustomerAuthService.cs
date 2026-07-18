using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Modules.Identity.Interfaces;
using POS.Application.Modules.Storefront.DTOs;
using POS.Domain.Modules.Orders.Entities;

namespace POS.Application.Modules.Storefront.Services;

/// <summary>
/// Customer identity for the storefront -- deliberately separate from staff Users/roles.
/// A customer only ever acts within one tenant's shop; there's no cross-tenant customer
/// account, and customers have no permissions (they can only ever act as themselves).
/// </summary>
public class CustomerAuthService
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext _readDb;
    private readonly ITokenService _tokenService;

    public CustomerAuthService(IWriteDbContext writeDb, IReadDbContext readDb, ITokenService tokenService)
    {
        _writeDb = writeDb;
        _readDb = readDb;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Check-then-create goes entirely through the write context: the existence check and
    /// the insert must see the same, current state, or a replica lagging by even a moment
    /// could let two registrations for the same email both pass the check (the DB's unique
    /// index on (TenantId, Email) is the real backstop against that race either way, but
    /// using the write context here also avoids a false "email taken" rejection right after
    /// someone's first attempt failed for an unrelated reason).
    /// </summary>
    public async Task<CustomerAuthResponse?> RegisterAsync(Guid tenantId, CustomerRegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();
        var exists = await _writeDb.Customers.AnyAsync(c => c.TenantId == tenantId && c.Email == email && !c.IsDeleted, ct);
        if (exists) return null;

        var customer = new Customer
        {
            TenantId = tenantId,
            FullName = request.FullName,
            Email = email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        _writeDb.Customers.Add(customer);
        await _writeDb.SaveChangesAsync(ct);

        var token = _tokenService.GenerateCustomerToken(customer, tenantId);
        return new CustomerAuthResponse(token, DateTime.UtcNow.AddDays(30), customer.FullName, tenantId);
    }

    /// <summary>Pure read -- read side (same login-after-registration lag trade-off as AuthService.LoginAsync).</summary>
    public async Task<CustomerAuthResponse?> LoginAsync(Guid tenantId, CustomerLoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();
        var customer = await _readDb.Customers.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Email == email && !c.IsDeleted, ct);

        if (customer is null || customer.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
            return null;

        var token = _tokenService.GenerateCustomerToken(customer, tenantId);
        return new CustomerAuthResponse(token, DateTime.UtcNow.AddDays(30), customer.FullName, tenantId);
    }
}
