using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Data
{
    public class SSCafeteriaDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public SSCafeteriaDbContext(DbContextOptions<SSCafeteriaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User -> Customer Orders
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.CustomerOrders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Admin -> Assigned Orders
            builder.Entity<Order>()
                .HasOne(o => o.Admin)
                .WithMany(u => u.AssignedOrders)
                .HasForeignKey(o => o.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category -> Products
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Order -> OrderItems
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            // Product -> OrderItems
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId);
        }
    }
}