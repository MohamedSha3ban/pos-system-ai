using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Domain.Modules.Identity.Entities;
using POS.Domain.Modules.Catalog.Entities;
using POS.Domain.Modules.Orders.Entities;

namespace POS.Infrastructure.Persistence;

/// <summary>
/// Read side of the CQRS-lite split. Points at the "Read" connection string -- in
/// production that's a read replica; for local dev without replica infrastructure, point
/// "Read" at the same database as "Write" in appsettings.json and everything still works
/// correctly, just without the actual scaling benefit until you add a real replica.
///
/// No-tracking by default: nothing fetched here is ever saved back through this context
/// (IReadDbContext doesn't even expose SaveChangesAsync), so tracking would only cost
/// memory/CPU for no benefit.
/// </summary>
public class ReadDbContext : DbContext, IReadDbContext
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public IQueryable<Tenant> Tenants => Set<Tenant>();
    public IQueryable<Location> Locations => Set<Location>();
    public IQueryable<User> Users => Set<User>();
    public IQueryable<Role> Roles => Set<Role>();
    public IQueryable<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();

    public IQueryable<Category> Categories => Set<Category>();
    public IQueryable<Product> Products => Set<Product>();
    public IQueryable<InventoryItem> InventoryItems => Set<InventoryItem>();

    public IQueryable<Customer> Customers => Set<Customer>();
    public IQueryable<Order> Orders => Set<Order>();
    public IQueryable<OrderItem> OrderItems => Set<OrderItem>();
    public IQueryable<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        PosModelConfiguration.Configure(modelBuilder);
    }
}
