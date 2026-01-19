using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace train.TempModels;

public partial class TempDbContext : DbContext
{
    public TempDbContext(DbContextOptions<TempDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }

    public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }

    public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }

    public virtual DbSet<CartItems> CartItems { get; set; }

    public virtual DbSet<Carts> Carts { get; set; }

    public virtual DbSet<Categories> Categories { get; set; }

    public virtual DbSet<OrderItems> OrderItems { get; set; }

    public virtual DbSet<Orders> Orders { get; set; }

    public virtual DbSet<Payments> Payments { get; set; }

    public virtual DbSet<Products> Products { get; set; }

    public virtual DbSet<ShippingMethods> ShippingMethods { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRoleClaims>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetRoles>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetUserClaims>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogins>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserTokens>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUsers>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Role).WithMany(p => p.User)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRoles",
                    r => r.HasOne<AspNetRoles>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUsers>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<CartItems>(entity =>
        {
            entity.HasIndex(e => new { e.CartId, e.ProductId }, "IX_CartItems_CartId_ProductId").IsUnique();

            entity.HasIndex(e => e.ProductId, "IX_CartItems_ProductId");

            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems).HasForeignKey(d => d.CartId);

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems).HasForeignKey(d => d.ProductId);
        });

        modelBuilder.Entity<Carts>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Carts_UserId");

            entity.HasIndex(e => e.AppusercontextId, "IX_Carts_appusercontextId");

            entity.Property(e => e.AppusercontextId).HasColumnName("appusercontextId");

            entity.HasOne(d => d.Appusercontext).WithMany(p => p.CartsAppusercontext).HasForeignKey(d => d.AppusercontextId);

            entity.HasOne(d => d.User).WithMany(p => p.CartsUser).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Categories>(entity =>
        {
            entity.HasIndex(e => e.ParentCategoryId, "IX_Categories_ParentCategoryId");

            entity.HasIndex(e => new { e.TargetAudience, e.Name, e.Color }, "IX_Categories_TargetAudience_Name_Color")
                .IsUnique()
                .HasFilter("([Color] IS NOT NULL)");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory).HasForeignKey(d => d.ParentCategoryId);
        });

        modelBuilder.Entity<OrderItems>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_OrderItems_OrderId");

            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasForeignKey(d => d.OrderId);
        });

        modelBuilder.Entity<Orders>(entity =>
        {
            entity.HasIndex(e => e.OrderDate, "IX_Orders_OrderDate");

            entity.HasIndex(e => e.Status, "IX_Orders_Status");

            entity.HasIndex(e => e.UserId, "IX_Orders_UserId");

            entity.Property(e => e.Discount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ShippingFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Payments>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_Payments_OrderId");

            entity.HasIndex(e => e.TransactionId, "IX_Payments_TransactionId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments).HasForeignKey(d => d.OrderId);
        });

        modelBuilder.Entity<Products>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_Products_CategoryId");

            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ShippingMethods>(entity =>
        {
            entity.Property(e => e.Cost).HasColumnType("decimal(18, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
