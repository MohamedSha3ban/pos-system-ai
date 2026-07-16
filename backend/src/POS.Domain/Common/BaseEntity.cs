namespace POS.Domain.Common;

/// <summary>
/// Base for every entity. TenantId enforces multi-tenant row isolation
/// (this POS is sold to many businesses, so every row must be scoped).
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; } = false; // soft delete
}
