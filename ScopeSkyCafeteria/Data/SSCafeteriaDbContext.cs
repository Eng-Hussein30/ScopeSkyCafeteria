using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Data
{
    public class SSCafeteriaDbContext: IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public SSCafeteriaDbContext(DbContextOptions<SSCafeteriaDbContext> options): base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Wallet> Wallets { get; set; }

        public DbSet<WalletTransaction> WalletTransactions { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasSequence<int>("OrderNumberSequence").StartsAt(1).IncrementsBy(1);

            builder.Entity<Order>()
                .Property(o => o.OrderNumber)
                .HasDefaultValueSql("NEXT VALUE FOR OrderNumberSequence");

            builder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();


            // =====================================================
            // User -> Customer Orders
            // =====================================================

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.CustomerOrders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // Admin -> Assigned Orders
            // =====================================================

            builder.Entity<Order>()
                .HasOne(o => o.Admin)
                .WithMany(u => u.AssignedOrders)
                .HasForeignKey(o => o.AdminId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // Category -> Products
            // =====================================================

            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);


            // =====================================================
            // Order -> OrderItems
            // =====================================================

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);


            // =====================================================
            // Product -> OrderItems
            // =====================================================

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId);


            // =====================================================
            // User -> Wallet
            // One User has One Wallet
            // =====================================================

            builder.Entity<Wallet>()
                .HasOne(w => w.User)
                .WithOne(u => u.Wallet)
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // UserId must be unique
            // So one user cannot have two wallets
            builder.Entity<Wallet>()
                .HasIndex(w => w.UserId)
                .IsUnique();


            // =====================================================
            // Wallet decimal precision
            // =====================================================

            builder.Entity<Wallet>()
                .Property(w => w.Balance)
                .HasPrecision(18, 2);

            builder.Entity<Wallet>()
                .Property(w => w.Debt)
                .HasPrecision(18, 2);


            // =====================================================
            // Wallet -> WalletTransactions
            // One Wallet has Many Transactions
            // =====================================================

            builder.Entity<WalletTransaction>()
                .HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);


            // =====================================================
            // WalletTransaction Amount
            // =====================================================

            builder.Entity<WalletTransaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);


            // =====================================================
            // WalletTransaction -> Order
            // =====================================================

            builder.Entity<WalletTransaction>()
                .HasOne(t => t.Order)
                .WithMany()
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // WalletTransaction -> PerformedByUser
            // Admin / SuperAdmin
            // =====================================================

            builder.Entity<WalletTransaction>()
                .HasOne(t => t.PerformedByUser)
                .WithMany()
                .HasForeignKey(t => t.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}