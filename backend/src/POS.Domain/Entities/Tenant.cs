using POS.Domain.Common;

namespace POS.Domain.Entities;

/// <summary>
/// Represents a business/merchant using the POS (this is the "multi-tenant" root).
/// </summary>
public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BusinessName { get; set; } = default!;
    public string BusinessType { get; set; } = "General"; // Retail, Restaurant, Service, General
    public string Currency { get; set; } = "USD";
    public string TimeZone { get; set; } = "UTC";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
