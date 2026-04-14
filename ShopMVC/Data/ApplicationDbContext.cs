using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShopMVC.Models;

namespace ShopMVC.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartDetail> CartDetails { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<vw_CartDetailsWithProduct> vw_CartDetailsWithProducts { get; set; }

    public virtual DbSet<vw_CartTotal> vw_CartTotals { get; set; }

    public virtual DbSet<vw_OrderDetailsWithProduct> vw_OrderDetailsWithProducts { get; set; }

    public virtual DbSet<vw_OrderTotal> vw_OrderTotals { get; set; }

    public virtual DbSet<vw_ProductsWithCategory> vw_ProductsWithCategories { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Carts__51BCD7B71B0907CE");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("Active");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Carts_Users");
        });

        modelBuilder.Entity<CartDetail>(entity =>
        {
            entity.HasKey(e => e.CartDetailId).HasName("PK__CartDeta__01B6A6B422AA2996");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartDetails).HasConstraintName("FK_CartDetails_Carts");

            entity.HasOne(d => d.Product).WithMany(p => p.CartDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartDetails_Products");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B7F60ED59");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCF2B3F6F97");

            entity.Property(e => e.OrderDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.OrderApprovedByNavigations).HasConstraintName("FK_Orders_ApprovedBy");

            entity.HasOne(d => d.User).WithMany(p => p.OrderUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Users");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D36C35BCFE0A");

            entity.ToTable(tb => tb.HasTrigger("trg_OrderDetails_SyncTotal"));

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails).HasConstraintName("FK_OrderDetails_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetails_Products");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CD108B795B");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C060DEAE8");

            entity.HasIndex(e => e.Email, "UX_Users_Email_NonNull")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<vw_CartDetailsWithProduct>(entity =>
        {
            entity.ToView("vw_CartDetailsWithProduct");
        });

        modelBuilder.Entity<vw_CartTotal>(entity =>
        {
            entity.ToView("vw_CartTotals");
        });

        modelBuilder.Entity<vw_OrderDetailsWithProduct>(entity =>
        {
            entity.ToView("vw_OrderDetailsWithProduct");
        });

        modelBuilder.Entity<vw_OrderTotal>(entity =>
        {
            entity.ToView("vw_OrderTotals");
        });

        modelBuilder.Entity<vw_ProductsWithCategory>(entity =>
        {
            entity.ToView("vw_ProductsWithCategory");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}