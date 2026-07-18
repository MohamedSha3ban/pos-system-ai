namespace POS.Domain.Modules.Identity.Entities;

/// <summary>
/// The Identity module's aggregate root for a business/merchant (the multi-tenant root).
/// Other modules reference Tenant only by Id (TenantId on BaseEntity) -- this keeps module
/// boundaries clean and would let Identity be split into its own service later.
/// </summary>
public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BusinessName { get; set; } = default!;
    public string BusinessType { get; set; } = "General";
    public string Currency { get; set; } = "USD";
    public string TimeZone { get; set; } = "UTC";
    public bool IsActive { get; set; } = true; // platform admin can deactivate a tenant
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
