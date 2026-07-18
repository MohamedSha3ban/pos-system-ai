using Microsoft.EntityFrameworkCore;
using POS.Domain.Modules.Identity.Entities;
using POS.Domain.Modules.Catalog.Entities;
using POS.Domain.Modules.Orders.Entities;

namespace POS.Application.Common.Interfaces;

/// <summary>
/// Shared persistence contract. Kept in Common (rather than per-module) because a single
/// ApplicationDbContext currently backs all modules -- see module READMEs / DependencyInjection.cs
/// for how to split this into per-module contexts if/when a module is extracted into its own service.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Location> Locations { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRoleAssignment> UserRoleAssignments { get; }

    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<InventoryItem> InventoryItems { get; }

    DbSet<Customer> Customers { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
