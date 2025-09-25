using DeliveryService.Domain;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Infrastructure;

public class DeliveryContext : DbContext
{
    public DbSet<DeliveryPerson> DeliveryPersons { get; set; } = null!;
    public DbSet<DeliveryOrder> DeliveryOrders { get; set; } = null!;

    public DeliveryContext(DbContextOptions<DeliveryContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure DeliveryPerson entity
        modelBuilder.Entity<DeliveryPerson>(entity =>
        {
            entity.ToTable("delivery_person");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);

            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure DeliveryOrder entity
        modelBuilder.Entity<DeliveryOrder>(entity =>
        {
            entity.ToTable("delivery_order");

            // Composite primary key
            entity.HasKey(e => new { e.OrderId, e.CustomerId });

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Category).HasColumnName("category").IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50)
                .HasDefaultValue("Available");
            entity.Property(e => e.DeliveryPersonId).HasColumnName("delivery_person_id");
            entity.Property(e => e.SnapshotJson).HasColumnName("snapshot").HasColumnType("jsonb");
            entity.Property(e => e.IdempotencyToken).HasColumnName("idempotency_token").HasMaxLength(255);

            // Ignore computed properties that shouldn't be mapped to database
            entity.Ignore(e => e.Snapshot);

            // Indexes
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DeliveryPersonId);
            entity.HasIndex(e => e.IdempotencyToken).IsUnique();

            // Foreign key relationship
            entity.HasOne(e => e.DeliveryPerson)
                .WithMany(dp => dp.DeliveryOrders)
                .HasForeignKey(e => e.DeliveryPersonId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}