using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Application.Common.Security;
using POS.Application.Modules.Identity.DTOs;
using POS.Application.Modules.Identity.Interfaces;
using POS.Domain.Common;
using POS.Domain.Modules.Identity.Entities;

namespace POS.Application.Modules.Identity.Services;

/// <summary>
/// Owns the staff refresh-token lifecycle: issuing a token pair on login, rotating on
/// refresh, revoking on logout, and listing active sessions. Deliberately uses
/// IWriteDbContext for everything except the independent "list my sessions" read --
/// refresh-token validation/rotation is exactly the kind of security-critical
/// check-then-write that must never operate against a possibly-stale replica (see
/// IWriteDbContext's doc comment for the same reasoning applied to checkout).
/// </summary>
public class SessionService
{
    private readonly IWriteDbContext _writeDb;
    private readonly IReadDbContext _readDb;
    private readonly ITokenService _tokenService;

    public SessionService(IWriteDbContext writeDb, IReadDbContext readDb, ITokenService tokenService)
    {
        _writeDb = writeDb;
        _readDb = readDb;
        _tokenService = tokenService;
    }

    /// <summary>Issues a new access+refresh token pair and persists the refresh token
    /// (hashed). Called by AuthService right after login or tenant registration.</summary>
    public async Task<AuthResponse> CreateSessionAsync(
        User user, List<string> roleNames, List<string> permissions,
        string? ip, string? userAgent, CancellationToken ct = default)
    {
        var (accessToken, accessExpiresAtUtc) = _tokenService.GenerateAccessToken(user, roleNames, permissions);
        var refreshTokenValue = RefreshTokenGenerator.GenerateToken();
        var refreshExpiresAtUtc = DateTime.UtcNow.Add(TokenLifetimes.StaffRefreshToken);

        _writeDb.RefreshTokens.Add(new RefreshToken
        {
            TenantId = user.TenantId,
            UserId = user.Id,
            TokenHash = RefreshTokenGenerator.Hash(refreshTokenValue),
            ExpiresAtUtc = refreshExpiresAtUtc,
            CreatedByIp = ip,
            UserAgent = userAgent
        });
        await _writeDb.SaveChangesAsync(ct);

        return new AuthResponse(accessToken, accessExpiresAtUtc, refreshTokenValue, refreshExpiresAtUtc,
            user.FullName, user.TenantId, user.IsPlatformAdmin, roleNames, permissions);
    }

    /// <summary>
    /// Rotates a refresh token: the presented token is revoked and a brand new one issued,
    /// alongside a fresh access token. If the presented token is already revoked -- meaning
    /// it was already used once before, or was explicitly logged out -- that's treated as a
    /// signal of possible theft/replay, and EVERY active session for that user is revoked as
    /// a precaution, forcing a fresh login everywhere. Unknown/expired tokens just fail
    /// quietly (null) with no side effects.
    /// </summary>
    public async Task<AuthResponse?> RefreshAsync(string refreshTokenValue, string? ip, string? userAgent, CancellationToken ct = default)
    {
        var hash = RefreshTokenGenerator.Hash(refreshTokenValue);
        var existing = await _writeDb.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (existing is null) return null;

        if (existing.RevokedAtUtc is not null)
        {
            // Reuse of an already-rotated/revoked token -- possible theft. Kill everything.
            await RevokeAllInternalAsync(existing.UserId, ct);
            return null;
        }

        if (DateTime.UtcNow >= existing.ExpiresAtUtc) return null;

        var user = await _writeDb.Users.FirstOrDefaultAsync(u => u.Id == existing.UserId && !u.IsDeleted && u.IsActive, ct);
        if (user is null) return null;

        var roleIds = await _writeDb.UserRoleAssignments.Where(a => a.UserId == user.Id).Select(a => a.RoleId).ToListAsync(ct);
        var roles = await _writeDb.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync(ct);
        var roleNames = roles.Select(r => r.Name).ToList();
        var permissions = roles.SelectMany(r => r.PermissionsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)).Distinct().ToList();

        var (accessToken, accessExpiresAtUtc) = _tokenService.GenerateAccessToken(user, roleNames, permissions);
        var newRefreshValue = RefreshTokenGenerator.GenerateToken();
        var newHash = RefreshTokenGenerator.Hash(newRefreshValue);
        var newExpiresAtUtc = DateTime.UtcNow.Add(TokenLifetimes.StaffRefreshToken);

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.ReplacedByTokenHash = newHash;

        _writeDb.RefreshTokens.Add(new RefreshToken
        {
            TenantId = user.TenantId,
            UserId = user.Id,
            TokenHash = newHash,
            ExpiresAtUtc = newExpiresAtUtc,
            CreatedByIp = ip,
            UserAgent = userAgent
        });

        await _writeDb.SaveChangesAsync(ct);

        return new AuthResponse(accessToken, accessExpiresAtUtc, newRefreshValue, newExpiresAtUtc,
            user.FullName, user.TenantId, user.IsPlatformAdmin, roleNames, permissions);
    }

    /// <summary>Logout: revokes exactly the session tied to the presented refresh token. No-op (true) if already gone/invalid -- logout is idempotent by design.</summary>
    public async Task<bool> RevokeAsync(string refreshTokenValue, CancellationToken ct = default)
    {
        var hash = RefreshTokenGenerator.Hash(refreshTokenValue);
        var existing = await _writeDb.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (existing is null) return true;

        if (existing.RevokedAtUtc is null)
        {
            existing.RevokedAtUtc = DateTime.UtcNow;
            await _writeDb.SaveChangesAsync(ct);
        }
        return true;
    }

    /// <summary>"Log out everywhere" -- revokes every active session for this user.</summary>
    public async Task<bool> RevokeAllAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        await RevokeAllInternalAsync(userId, ct, tenantId);
        return true;
    }

    /// <summary>Independent list read -- read side (see class doc comment for why this one differs from the rest).</summary>
    public async Task<List<SessionDto>> GetActiveSessionsAsync(Guid tenantId, Guid userId, string? currentRefreshTokenValue, CancellationToken ct = default)
    {
        var currentHash = currentRefreshTokenValue is not null ? RefreshTokenGenerator.Hash(currentRefreshTokenValue) : null;
        var now = DateTime.UtcNow;

        var sessions = await _readDb.RefreshTokens
            .Where(t => t.TenantId == tenantId && t.UserId == userId && t.RevokedAtUtc == null && t.ExpiresAtUtc > now)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);

        return sessions.Select(t => new SessionDto(
            t.Id, t.CreatedAtUtc, t.ExpiresAtUtc, t.CreatedByIp, t.UserAgent, t.TokenHash == currentHash
        )).ToList();
    }

    private async Task RevokeAllInternalAsync(Guid userId, CancellationToken ct, Guid? tenantId = null)
    {
        var query = _writeDb.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAtUtc == null);
        if (tenantId.HasValue) query = query.Where(t => t.TenantId == tenantId.Value);

        var active = await query.ToListAsync(ct);
        foreach (var token in active)
            token.RevokedAtUtc = DateTime.UtcNow;

        await _writeDb.SaveChangesAsync(ct);
    }
}
