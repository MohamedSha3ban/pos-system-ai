using POS.Domain.Common;

namespace POS.Domain.Modules.Identity.Entities;

/// <summary>
/// One row = one staff login session. The raw token is never stored -- only its SHA-256
/// hash (TokenHash), so a database read/leak doesn't hand out usable credentials. Rotation
/// chain: every time a token is used to refresh, this row is revoked and ReplacedByTokenHash
/// points at the new row's hash, so a stolen-and-replayed old token is detectable (see
/// SessionService.RefreshAsync) even after it's no longer the "current" token for the user.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? CreatedByIp { get; set; }
    public string? UserAgent { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
