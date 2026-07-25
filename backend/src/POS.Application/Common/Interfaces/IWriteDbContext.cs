using Microsoft.EntityFrameworkCore;
using POS.Domain.Modules.Identity.Entities;
using POS.Domain.Modules.Catalog.Entities;
using POS.Domain.Modules.Orders.Entities;

namespace POS.Application.Common.Interfaces;

/// <summary>
/// Write side of the CQRS-lite split. Exposes DbSet&lt;T&gt; (so Add/Update/Remove are
/// available) plus SaveChangesAsync. Backed by WriteDbContext, which always points at the
/// primary database.
///
/// Used for every mutation, AND for any read that must be immediately consistent with a
/// mutation in the same operation -- e.g. OrderService reads product prices/stock through
/// here (not IReadDbContext) because checkout must never decide "is this in stock?" against
/// a replica that might be a few hundred milliseconds behind the primary. Same reasoning
/// applies to CustomerAuthService's registration flow (check-email-then-create) and to the
/// Create/Update methods across the other services, which build their response DTOs from
/// entities already in this context's change tracker rather than re-querying the read side.
/// </summary>
public interface IWriteDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Location> Locations { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRoleAssignment> UserRoleAssignments { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<InventoryItem> InventoryItems { get; }

    DbSet<Customer> Customers { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<CustomerRefreshToken> CustomerRefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
