using POS.Domain.Common;

namespace POS.Domain.Modules.Orders.Entities;

/// <summary>
/// Customer-side mirror of Identity's RefreshToken -- kept as a separate entity rather than
/// shared/generic, consistent with how Customer is a deliberately separate identity from
/// staff User throughout this codebase (see Storefront module). Same hashing and rotation
/// design as RefreshToken.
/// </summary>
public class CustomerRefreshToken : BaseEntity
{
    public Guid CustomerId { get; set; }
    public string TokenHash { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? CreatedByIp { get; set; }
    public string? UserAgent { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
