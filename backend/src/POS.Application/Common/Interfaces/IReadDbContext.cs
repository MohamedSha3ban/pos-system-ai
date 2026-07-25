using POS.Domain.Modules.Identity.Entities;
using POS.Domain.Modules.Catalog.Entities;
using POS.Domain.Modules.Orders.Entities;

namespace POS.Application.Common.Interfaces;

/// <summary>
/// Read side of the CQRS-lite split. Every property is IQueryable&lt;T&gt;, not
/// DbSet&lt;T&gt; -- there is no Add/Update/Remove/SaveChanges reachable through this
/// interface at all, so a read-only service literally cannot accidentally write. Backed by
/// ReadDbContext, which in production points at a read replica (see appsettings.json /
/// README) and queries with no change tracking by default.
///
/// Used for independent list/browse/reporting reads that don't need to be consistent with
/// a write from the same request -- product catalogs, inventory listings, tenant summaries,
/// the AI forecasting service, login lookups. If a read needs read-your-own-write
/// consistency within a single operation, it goes through IWriteDbContext instead (see that
/// interface's doc comment for examples).
/// </summary>
public interface IReadDbContext
{
    IQueryable<Tenant> Tenants { get; }
    IQueryable<Location> Locations { get; }
    IQueryable<User> Users { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<UserRoleAssignment> UserRoleAssignments { get; }
    IQueryable<RefreshToken> RefreshTokens { get; }

    IQueryable<Category> Categories { get; }
    IQueryable<Product> Products { get; }
    IQueryable<InventoryItem> InventoryItems { get; }

    IQueryable<Customer> Customers { get; }
    IQueryable<Order> Orders { get; }
    IQueryable<OrderItem> OrderItems { get; }
    IQueryable<Payment> Payments { get; }
    IQueryable<CustomerRefreshToken> CustomerRefreshTokens { get; }
}
