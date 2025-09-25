using Microsoft.EntityFrameworkCore;
using GraphOfOrders.Lib.Entities;

namespace GraphOfOrders.Repo
{
    public class OrdersContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<DeliverPerson> DeliverPersons { get; set; }

        public OrdersContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryName).IsRequired();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductName).IsRequired();
                entity.HasOne(d => d.Category)
                      .WithMany(p => p.Products)
                      .HasForeignKey(d => d.CategoryId);
            });

            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(e => e.BrandId);
                entity.Property(e => e.BrandName).IsRequired();
                entity.HasOne(d => d.Product)
                     .WithMany(p => p.Brands)
                     .HasForeignKey(d => d.ProductId);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.OrderDate).IsRequired(); // May not be necessary as DateTime is non-nullable
                entity.Property(e => e.DeliveryStatus).HasDefaultValue("Pending");
                entity.HasOne(d => d.Brand)
                    .WithMany(p => p.OrdersRecords)
                    .HasForeignKey(d => d.BrandId);
                entity.HasOne(d => d.Customer)
                    .WithMany(b => b.OrdersHistory)
                    .HasForeignKey(d => d.CustomerId);
                entity.HasOne(d => d.DeliverPerson)
                    .WithMany(dp => dp.Orders)
                    .HasForeignKey(d => d.DeliverPersonId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.Email).IsRequired();
            });

            modelBuilder.Entity<DeliverPerson>(entity =>
            {
                entity.HasKey(e => e.DeliverPersonId);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Phone);
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            });
        }
    }

}
