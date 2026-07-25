using Microsoft.EntityFrameworkCore;
using POS.Application.Common.Interfaces;
using POS.Domain.Modules.Identity.Entities;
using POS.Domain.Modules.Catalog.Entities;
using POS.Domain.Modules.Orders.Entities;

namespace POS.Infrastructure.Persistence;

/// <summary>
/// Write side of the CQRS-lite split -- always the primary database (see
/// DependencyInjection.cs / appsettings.json "Write" connection string).
///
/// This is also the ONLY context with migrations (see Migrations/). ReadDbContext's data
/// is expected to arrive via database-level replication (e.g. Postgres streaming
/// replication) in production, not EF migrations -- running `dotnet ef migrations add`
/// always targets this context (--context WriteDbContext), never ReadDbContext.
/// </summary>
public class WriteDbContext : DbContext, IWriteDbContext
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CustomerRefreshToken> CustomerRefreshTokens => Set<CustomerRefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        PosModelConfiguration.Configure(modelBuilder);
    }
}
