using EpsilonWebApp.Contracts.Models;
using Microsoft.EntityFrameworkCore;

namespace EpsilonWebApp.Infrastructure.Repositories.EFCore.Configurations
{
    public class CustomersDbContext : DbContext
    {
        public CustomersDbContext(DbContextOptions<CustomersDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompanyName).HasMaxLength(200);
                entity.Property(e => e.ContactName).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Region).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(50);
            });

            // Seed data
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    CompanyName = "Acme Corporation",
                    ContactName = "John Doe",
                    Address = "123 Main Street",
                    City = "New York",
                    Region = "NY",
                    PostalCode = "10001",
                    Country = "USA",
                    Phone = "+1-555-0100"
                },
                new Customer
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    CompanyName = "Tech Solutions Ltd",
                    ContactName = "Jane Smith",
                    Address = "456 Tech Avenue",
                    City = "San Francisco",
                    Region = "CA",
                    PostalCode = "94102",
                    Country = "USA",
                    Phone = "+1-555-0200"
                },
                new Customer
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    CompanyName = "Global Trading Inc",
                    ContactName = "Bob Johnson",
                    Address = "789 Commerce Blvd",
                    City = "London",
                    Region = "Greater London",
                    PostalCode = "SW1A 1AA",
                    Country = "UK",
                    Phone = "+44-20-7946-0958"
                }
            );
        }
    }
}
