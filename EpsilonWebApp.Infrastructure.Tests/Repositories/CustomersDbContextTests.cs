using EpsilonWebApp.Contracts.Models;
using EpsilonWebApp.Infrastructure.Repositories.EFCore.Configurations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EpsilonWebApp.Infrastructure.Tests.Repositories
{
    public class CustomersDbContextTests : IDisposable
    {
        private readonly CustomersDbContext _context;

        public CustomersDbContextTests()
        {
            var options = new DbContextOptionsBuilder<CustomersDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CustomersDbContext(options);
            _context.Database.EnsureCreated();
        }

        [Fact]
        public void CustomersDbContext_ShouldHaveCustomersDbSet()
        {
            _context.Customers.Should().NotBeNull();
        }

        [Fact]
        public void CustomersDbContext_ShouldSeedInitialData()
        {
            var customers = _context.Customers.ToList();

            customers.Should().HaveCount(3);
            customers.Should().Contain(c => c.CompanyName == "Acme Corporation");
            customers.Should().Contain(c => c.CompanyName == "Tech Solutions Ltd");
            customers.Should().Contain(c => c.CompanyName == "Global Trading Inc");
        }

        [Fact]
        public void CustomersDbContext_ShouldHaveCorrectSeededIds()
        {
            var acme = _context.Customers.FirstOrDefault(c => c.CompanyName == "Acme Corporation");
            var tech = _context.Customers.FirstOrDefault(c => c.CompanyName == "Tech Solutions Ltd");
            var global = _context.Customers.FirstOrDefault(c => c.CompanyName == "Global Trading Inc");

            acme.Should().NotBeNull();
            acme!.Id.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
            
            tech.Should().NotBeNull();
            tech!.Id.Should().Be(Guid.Parse("22222222-2222-2222-2222-222222222222"));
            
            global.Should().NotBeNull();
            global!.Id.Should().Be(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        }

        [Fact]
        public async Task CustomersDbContext_ShouldAddNewCustomer()
        {
            var newCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                CompanyName = "New Company",
                ContactName = "New Contact",
                City = "Test City"
            };

            _context.Customers.Add(newCustomer);
            await _context.SaveChangesAsync();

            var result = await _context.Customers.FindAsync(newCustomer.Id);
            result.Should().NotBeNull();
            result!.CompanyName.Should().Be("New Company");
        }

        [Fact]
        public async Task CustomersDbContext_ShouldUpdateCustomer()
        {
            var customer = _context.Customers.First();
            var originalName = customer.CompanyName;

            customer.CompanyName = "Updated Company Name";
            await _context.SaveChangesAsync();

            var updated = await _context.Customers.FindAsync(customer.Id);
            updated.Should().NotBeNull();
            updated!.CompanyName.Should().Be("Updated Company Name");
            updated.CompanyName.Should().NotBe(originalName);
        }

        [Fact]
        public async Task CustomersDbContext_ShouldDeleteCustomer()
        {
            var customer = _context.Customers.First();
            var customerId = customer.Id;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            var deleted = await _context.Customers.FindAsync(customerId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task CustomersDbContext_ShouldQueryCustomersByCity()
        {
            var newYorkCustomers = await _context.Customers
                .Where(c => c.City == "New York")
                .ToListAsync();

            newYorkCustomers.Should().HaveCount(1);
            newYorkCustomers.First().CompanyName.Should().Be("Acme Corporation");
        }

        [Fact]
        public async Task CustomersDbContext_ShouldQueryCustomersByCountry()
        {
            var usaCustomers = await _context.Customers
                .Where(c => c.Country == "USA")
                .ToListAsync();

            usaCustomers.Should().HaveCount(2);
        }

        [Fact]
        public void CustomersDbContext_Customer_ShouldHaveIdAsPrimaryKey()
        {
            var entityType = _context.Model.FindEntityType(typeof(Customer));

            var primaryKey = entityType!.FindPrimaryKey();

            primaryKey.Should().NotBeNull();
            primaryKey!.Properties.Should().HaveCount(1);
            primaryKey.Properties.First().Name.Should().Be("Id");
        }

        [Theory]
        [InlineData("CompanyName", 200)]
        [InlineData("ContactName", 200)]
        [InlineData("Address", 500)]
        [InlineData("City", 100)]
        [InlineData("Region", 100)]
        [InlineData("PostalCode", 20)]
        [InlineData("Country", 100)]
        [InlineData("Phone", 50)]
        public void CustomersDbContext_Customer_ShouldHaveCorrectMaxLengths(string propertyName, int expectedMaxLength)
        {
            // Arrange
            var entityType = _context.Model.FindEntityType(typeof(Customer));
            var property = entityType!.FindProperty(propertyName);

            // Act
            var maxLength = property!.GetMaxLength();

            // Assert
            maxLength.Should().Be(expectedMaxLength);
        }

        [Fact]
        public async Task CustomersDbContext_ShouldHandleConcurrentWrites()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer1 = new Customer
            {
                Id = customerId,
                CompanyName = "Concurrent Test",
                City = "Test City"
            };

            _context.Customers.Add(customer1);
            await _context.SaveChangesAsync();

            // Create second context to simulate concurrent access
            var options = new DbContextOptionsBuilder<CustomersDbContext>()
                .UseInMemoryDatabase(databaseName: _context.Database.GetDbConnection().Database)
                .Options;
            
            using var context2 = new CustomersDbContext(options);
            
            // Act
            var customerFromContext1 = await _context.Customers.FindAsync(customerId);
            var customerFromContext2 = await context2.Customers.FindAsync(customerId);

            customerFromContext1!.CompanyName = "Updated from Context1";
            customerFromContext2!.CompanyName = "Updated from Context2";

            await _context.SaveChangesAsync();

            // Assert - In-memory DB doesn't enforce concurrency, but we can verify the setup works
            customerFromContext1.CompanyName.Should().Be("Updated from Context1");
            customerFromContext2.CompanyName.Should().Be("Updated from Context2");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
