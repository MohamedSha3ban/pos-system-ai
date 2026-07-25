using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Common.Security;
using POS.Application.Modules.Identity.Interfaces;
using POS.Application.Modules.Storefront.DTOs;
using POS.Domain.Common;
using POS.Domain.Modules.Orders.Entities;

namespace POS.Application.Modules.Storefront.Services;

/// <summary>
/// Customer-side mirror of Identity's SessionService -- same rotation-with-reuse-detection
/// design, same write-context-only reasoning for anything security-critical, kept as a
/// separate implementation rather than a shared generic because Customer is a deliberately
/// separate identity from staff User throughout this codebase.
/// </summary>
public class CustomerSessionService
{
    private readonly IWriteDbContext _writeDb;
    private readonly ITokenService _tokenService;

    public CustomerSessionService(IWriteDbContext writeDb, ITokenService tokenService)
    {
        _writeDb = writeDb;
        _tokenService = tokenService;
    }

    public async Task<CustomerAuthResponse> CreateSessionAsync(Customer customer, Guid tenantId, string? ip, string? userAgent, CancellationToken ct = default)
    {
        var (accessToken, accessExpiresAtUtc) = _tokenService.GenerateCustomerAccessToken(customer, tenantId);
        var refreshTokenValue = RefreshTokenGenerator.GenerateToken();
        var refreshExpiresAtUtc = DateTime.UtcNow.Add(TokenLifetimes.CustomerRefreshToken);

        _writeDb.CustomerRefreshTokens.Add(new CustomerRefreshToken
        {
            TenantId = tenantId,
            CustomerId = customer.Id,
            TokenHash = RefreshTokenGenerator.Hash(refreshTokenValue),
            ExpiresAtUtc = refreshExpiresAtUtc,
            CreatedByIp = ip,
            UserAgent = userAgent
        });
        await _writeDb.SaveChangesAsync(ct);

        return new CustomerAuthResponse(accessToken, accessExpiresAtUtc, refreshTokenValue, refreshExpiresAtUtc, customer.FullName, tenantId);
    }

    /// <summary>Same rotation + reuse-detection design as SessionService.RefreshAsync -- see its doc comment.</summary>
    public async Task<CustomerAuthResponse?> RefreshAsync(string refreshTokenValue, string? ip, string? userAgent, CancellationToken ct = default)
    {
        var hash = RefreshTokenGenerator.Hash(refreshTokenValue);
        var existing = await _writeDb.CustomerRefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (existing is null) return null;

        if (existing.RevokedAtUtc is not null)
        {
            var allActive = await _writeDb.CustomerRefreshTokens
                .Where(t => t.CustomerId == existing.CustomerId && t.RevokedAtUtc == null)
                .ToListAsync(ct);
            foreach (var token in allActive) token.RevokedAtUtc = DateTime.UtcNow;
            await _writeDb.SaveChangesAsync(ct);
            return null;
        }

        if (DateTime.UtcNow >= existing.ExpiresAtUtc) return null;

        var customer = await _writeDb.Customers.FirstOrDefaultAsync(c => c.Id == existing.CustomerId && !c.IsDeleted, ct);
        if (customer is null) return null;

        var (accessToken, accessExpiresAtUtc) = _tokenService.GenerateCustomerAccessToken(customer, existing.TenantId);
        var newRefreshValue = RefreshTokenGenerator.GenerateToken();
        var newHash = RefreshTokenGenerator.Hash(newRefreshValue);
        var newExpiresAtUtc = DateTime.UtcNow.Add(TokenLifetimes.CustomerRefreshToken);

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.ReplacedByTokenHash = newHash;

        _writeDb.CustomerRefreshTokens.Add(new CustomerRefreshToken
        {
            TenantId = existing.TenantId,
            CustomerId = customer.Id,
            TokenHash = newHash,
            ExpiresAtUtc = newExpiresAtUtc,
            CreatedByIp = ip,
            UserAgent = userAgent
        });

        await _writeDb.SaveChangesAsync(ct);

        return new CustomerAuthResponse(accessToken, accessExpiresAtUtc, newRefreshValue, newExpiresAtUtc, customer.FullName, existing.TenantId);
    }

    /// <summary>Logout: idempotent, revokes exactly the session tied to the presented refresh token.</summary>
    public async Task<bool> RevokeAsync(string refreshTokenValue, CancellationToken ct = default)
    {
        var hash = RefreshTokenGenerator.Hash(refreshTokenValue);
        var existing = await _writeDb.CustomerRefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (existing is null) return true;

        if (existing.RevokedAtUtc is null)
        {
            existing.RevokedAtUtc = DateTime.UtcNow;
            await _writeDb.SaveChangesAsync(ct);
        }
        return true;
    }
}
