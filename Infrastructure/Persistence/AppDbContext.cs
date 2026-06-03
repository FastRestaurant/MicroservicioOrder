using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderNote> OrderNotes => Set<OrderNote>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(o => o.Id);

            e.Property(o => o.Id).HasColumnType("uniqueidentifier");
            e.Property(o => o.TableNumber).IsRequired();
            e.Property(o => o.WaiterId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(o => o.Status).HasMaxLength(30).IsRequired();
            e.Property(o => o.Total).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(o => o.CreatedAt).IsRequired();
            e.Property(o => o.UpdatedAt).IsRequired();
            e.Property(o => o.ClosedAt);

            e.Property(o => o.Version)
             .IsRowVersion();

            e.HasMany(o => o.Items)
             .WithOne(i => i.Order)
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(o => o.Notes)
             .WithOne(n => n.Order)
             .HasForeignKey(n => n.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(o => o.StatusHistory)
             .WithOne(h => h.Order)
             .HasForeignKey(h => h.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(o => o.Status);
            e.HasIndex(o => o.TableNumber);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems");
            e.HasKey(i => i.Id);

            e.Property(i => i.Id).HasColumnType("uniqueidentifier");
            e.Property(i => i.OrderId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(i => i.ProductId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(i => i.ProductType).HasMaxLength(50).IsRequired();
            e.Property(i => i.ProductNameSnapshot).HasMaxLength(200).IsRequired();
            e.Property(i => i.UnitPriceSnapshot).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(i => i.Quantity).IsRequired();
            e.Property(i => i.Status).HasMaxLength(30).IsRequired();
            e.Property(i => i.Notes).HasMaxLength(500);
            e.Property(i => i.SentToKitchenAt);
            e.Property(i => i.ReadyAt);
            e.Property(i => i.CreatedAt).IsRequired();
            e.Property(i => i.UpdatedAt).IsRequired();

            e.HasIndex(i => i.OrderId);
            e.HasIndex(i => i.Status);
        });

        modelBuilder.Entity<OrderNote>(e =>
        {
            e.ToTable("OrderNotes");
            e.HasKey(n => n.Id);

            e.Property(n => n.Id).HasColumnType("uniqueidentifier");
            e.Property(n => n.OrderId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(n => n.CreatedByUserId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(n => n.Note).HasMaxLength(1000).IsRequired();
            e.Property(n => n.CreatedAt).IsRequired();

            e.HasIndex(n => n.OrderId);
        });


        modelBuilder.Entity<OrderStatusHistory>(e =>
        {
            e.ToTable("OrderStatusHistories");
            e.HasKey(h => h.Id);

            e.Property(h => h.Id).HasColumnType("uniqueidentifier");
            e.Property(h => h.OrderId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(h => h.PreviousStatus).HasMaxLength(30);
            e.Property(h => h.NewStatus).HasMaxLength(30).IsRequired();
            e.Property(h => h.ChangedByUserId).HasColumnType("uniqueidentifier").IsRequired();
            e.Property(h => h.ChangedAt).IsRequired();

            e.HasIndex(h => h.OrderId);
        });
    }
}
