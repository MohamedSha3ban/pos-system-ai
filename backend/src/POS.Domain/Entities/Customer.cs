using POS.Domain.Common;

namespace POS.Domain.Entities;

public class Customer : BaseEntity
{
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int LoyaltyPoints { get; set; } = 0;
}
