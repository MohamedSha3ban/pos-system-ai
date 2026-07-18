using POS.Domain.Common;

namespace POS.Domain.Modules.Orders.Entities;

public class Customer : BaseEntity
{
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int LoyaltyPoints { get; set; } = 0;

    /// <summary>
    /// Set only for customers who've registered an account on the storefront
    /// (see Storefront module). Null for walk-in customers a cashier attaches to an
    /// in-store order without the customer ever logging in anywhere.
    /// </summary>
    public string? PasswordHash { get; set; }
}
