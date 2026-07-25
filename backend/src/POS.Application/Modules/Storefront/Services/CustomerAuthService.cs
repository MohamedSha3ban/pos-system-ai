using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
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
    private readonly CustomerSessionService _sessionService;

    public CustomerAuthService(IWriteDbContext writeDb, IReadDbContext readDb, CustomerSessionService sessionService)
    {
        _writeDb = writeDb;
        _readDb = readDb;
        _sessionService = sessionService;
    }

    /// <summary>
    /// Check-then-create goes entirely through the write context: the existence check and
    /// the insert must see the same, current state (the DB's unique index on (TenantId,
    /// Email) is the real backstop against a race either way, but using the write context
    /// here also avoids a false "email taken" rejection right after someone's first attempt
    /// failed for an unrelated reason).
    /// </summary>
    public async Task<CustomerAuthResponse?> RegisterAsync(Guid tenantId, CustomerRegisterRequest request, string? ip, string? userAgent, CancellationToken ct = default)
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

        return await _sessionService.CreateSessionAsync(customer, tenantId, ip, userAgent, ct);
    }

    /// <summary>Pure read -- read side (same login-after-registration lag trade-off as AuthService.LoginAsync).</summary>
    public async Task<CustomerAuthResponse?> LoginAsync(Guid tenantId, CustomerLoginRequest request, string? ip, string? userAgent, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();
        var customer = await _readDb.Customers.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Email == email && !c.IsDeleted, ct);

        if (customer is null || customer.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
            return null;

        return await _sessionService.CreateSessionAsync(customer, tenantId, ip, userAgent, ct);
    }
}
