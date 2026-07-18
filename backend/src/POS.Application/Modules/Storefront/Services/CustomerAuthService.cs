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
    private readonly IApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public CustomerAuthService(IApplicationDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<CustomerAuthResponse?> RegisterAsync(Guid tenantId, CustomerRegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();
        var exists = await _db.Customers.AnyAsync(c => c.TenantId == tenantId && c.Email == email && !c.IsDeleted, ct);
        if (exists) return null;

        var customer = new Customer
        {
            TenantId = tenantId,
            FullName = request.FullName,
            Email = email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);

        var token = _tokenService.GenerateCustomerToken(customer, tenantId);
        return new CustomerAuthResponse(token, DateTime.UtcNow.AddDays(30), customer.FullName, tenantId);
    }

    public async Task<CustomerAuthResponse?> LoginAsync(Guid tenantId, CustomerLoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Email == email && !c.IsDeleted, ct);

        if (customer is null || customer.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
            return null;

        var token = _tokenService.GenerateCustomerToken(customer, tenantId);
        return new CustomerAuthResponse(token, DateTime.UtcNow.AddDays(30), customer.FullName, tenantId);
    }
}
