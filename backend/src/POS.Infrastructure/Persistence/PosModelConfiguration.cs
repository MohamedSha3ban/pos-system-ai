using Microsoft.EntityFrameworkCore;
using POS.Domain.Modules.Identity.Entities;
using POS.Domain.Modules.Catalog.Entities;
using POS.Domain.Modules.Orders.Entities;

namespace POS.Infrastructure.Persistence;

/// <summary>
/// Entity mapping shared by both WriteDbContext and ReadDbContext -- the read side is a
/// replica of the exact same schema, so this configuration lives in one place rather than
/// being duplicated across two OnModelCreating overrides and risking drift between them.
/// </summary>
public static class PosModelConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        // ReadDbContext exposes IQueryable<T>, not DbSet<T> -- EF's default entity
        // discovery convention looks for DbSet<T> properties, so without these explicit
        // registrations ReadDbContext would silently fail to discover several entity
        // types (any not otherwise referenced by a fluent HasOne/HasIndex/etc. call
        // below). Registering every entity explicitly makes discovery correct regardless
        // of which CLR property type the context exposes it as.
        modelBuilder.Entity<Tenant>();
        modelBuilder.Entity<Location>();
        modelBuilder.Entity<User>();
        modelBuilder.Entity<Role>();
        modelBuilder.Entity<UserRoleAssignment>();
        modelBuilder.Entity<RefreshToken>();
        modelBuilder.Entity<Category>();
        modelBuilder.Entity<Product>();
        modelBuilder.Entity<InventoryItem>();
        modelBuilder.Entity<Customer>();
        modelBuilder.Entity<Order>();
        modelBuilder.Entity<OrderItem>();
        modelBuilder.Entity<Payment>();
        modelBuilder.Entity<CustomerRefreshToken>();

        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);

        modelBuilder.Entity<Product>().HasIndex(p => new { p.TenantId, p.Sku }).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<InventoryItem>().HasIndex(i => new { i.ProductId, i.LocationId }).IsUnique();
        modelBuilder.Entity<Role>().HasIndex(r => new { r.TenantId, r.Name }).IsUnique();
        modelBuilder.Entity<UserRoleAssignment>().HasIndex(a => new { a.UserId, a.RoleId }).IsUnique();
        modelBuilder.Entity<Customer>().HasIndex(c => new { c.TenantId, c.Email })
            .IsUnique()
            .HasFilter("\"Email\" IS NOT NULL");

        // TokenHash lookups happen on every refresh call -- unique + indexed for O(log n)
        // lookup instead of a table scan. UserId/CustomerId indexes back the "list my
        // active sessions" queries.
        modelBuilder.Entity<RefreshToken>().HasIndex(t => t.TokenHash).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasIndex(t => t.UserId);
        modelBuilder.Entity<CustomerRefreshToken>().HasIndex(t => t.TokenHash).IsUnique();
        modelBuilder.Entity<CustomerRefreshToken>().HasIndex(t => t.CustomerId);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<InventoryItem>()
            .HasOne(i => i.Product)
            .WithMany(p => p.InventoryItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Payments)
            .WithOne()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRoleAssignment>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRoleAssignment>()
            .HasOne<Role>()
            .WithMany()
            .HasForeignKey(a => a.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetPrecision(18);
                    property.SetScale(2);
                }
            }
        }
    }
}
