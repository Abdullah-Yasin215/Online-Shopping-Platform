using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using train.Models;

namespace train.Areas.Identity.Data
{
    public class appdbcontext : IdentityDbContext<appusercontext>
    {
        public appdbcontext(DbContextOptions<appdbcontext> options) : base(options) { }

        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Cart> Carts { get; set; } = default!;
        public DbSet<CartItem> CartItems { get; set; } = default!;
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<OrderItem> OrderItems { get; set; } = default!;


        public DbSet<Payment> Payments { get; set; }
        public DbSet<ShippingMethod> ShippingMethods { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Payment>(entity =>
            {
                entity.HasIndex(p => p.OrderId);
                entity.HasIndex(p => p.TransactionId);
                entity.Property(p => p.Amount).HasPrecision(18, 2);
            });

            builder.Entity<ShippingMethod>(entity =>
            {
                entity.Property(s => s.Cost).HasPrecision(18, 2);
            }); builder.Entity<Order>(e =>
            {
                e.HasOne(o => o.User)
                 .WithMany()                       // no back-collection on user
                 .HasForeignKey(o => o.UserId)     // FK: Order.UserId
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.SetNull);// keep orders when a user is deleted

                // Money precision for all monetary fields
                e.Property(o => o.Subtotal).HasColumnType("decimal(18,2)");
                e.Property(o => o.ShippingFee).HasColumnType("decimal(18,2)");
                e.Property(o => o.Discount).HasColumnType("decimal(18,2)");
                e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");

                // Optional: index by most common queries
                e.HasIndex(o => o.OrderDate);
                e.HasIndex(o => o.Status);
            });

            // --------------------------------
            // Category self relation + indexes
            // --------------------------------
            builder.Entity<Category>()
                .HasMany(c => c.SubCategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Category>()
                .HasIndex(c => new { c.TargetAudience, c.Name, c.Color })
                .IsUnique();

            // --------------------
            // Product → Category
            // --------------------
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // ---------------
            // Cart + Items
            // ---------------
            builder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique();

            builder.Entity<Cart>()
                .HasOne<appusercontext>() // optional user link on cart
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------
            // Orders + Items
            // -------------
            builder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)");

            builder.ApplyConfiguration(new ApplicationUserEntityConfiguration());
        }
    }

    public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<appusercontext>
    {
        public void Configure(EntityTypeBuilder<appusercontext> builder)
        {
            builder.Property(x => x.City);
            builder.Property(x => x.Age);
        }
    }
}
