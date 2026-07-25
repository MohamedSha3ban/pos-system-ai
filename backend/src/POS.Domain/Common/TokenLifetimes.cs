namespace POS.Domain.Common;

/// <summary>
/// Single source of truth for access/refresh token lifetimes -- used by JwtTokenService
/// (to set the JWT "exp" claim) and by the session services (to set the matching
/// ExpiresAtUtc on the persisted RefreshToken/CustomerRefreshToken row). Access tokens are
/// short-lived by design: logout/revocation works by deleting the refresh token's ability
/// to mint new access tokens, NOT by blacklisting already-issued JWTs (which would need a
/// distributed cache to check on every request). A short access-token TTL is what makes
/// that trade-off acceptable -- a revoked session's last access token simply expires on
/// its own within minutes instead of being usable for hours.
/// </summary>
public static class TokenLifetimes
{
    public static readonly TimeSpan StaffAccessToken = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan StaffRefreshToken = TimeSpan.FromDays(7);

    public static readonly TimeSpan CustomerAccessToken = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan CustomerRefreshToken = TimeSpan.FromDays(30);
}
