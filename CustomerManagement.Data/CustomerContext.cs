using Microsoft.EntityFrameworkCore;
using CustomerManagement.Models;

namespace CustomerManagement.Data
{
    public class CustomerContext : DbContext
    {
        public CustomerContext(DbContextOptions<CustomerContext> options) : base(options) { }
        
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.CustomerId);
                entity.HasIndex(c => c.Email).IsUnique();
                entity.Property(c => c.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });
            
            // Address configuration
            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(a => a.AddressId);
                entity.HasOne(a => a.Customer)
                      .WithMany(c => c.Addresses)
                      .HasForeignKey(a => a.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);
                entity.HasIndex(o => o.OrderNumber).IsUnique();
                entity.Property(o => o.OrderDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            });
            
            // Many-to-Many: CustomerOrder configuration
            modelBuilder.Entity<CustomerOrder>(entity =>
            {
                entity.HasKey(co => new { co.CustomerId, co.OrderId });
                
                entity.HasOne(co => co.Customer)
                      .WithMany(c => c.CustomerOrders)
                      .HasForeignKey(co => co.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(co => co.Order)
                      .WithMany(o => o.CustomerOrders)
                      .HasForeignKey(co => co.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.Property(co => co.AssignedDate).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
