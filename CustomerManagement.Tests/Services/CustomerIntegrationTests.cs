using CustomerManagement.Data;
using CustomerManagement.Models;
using CustomerManagement.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerManagement.Tests.Integration
{
    public class CustomerIntegrationTests : IClassFixture<CustomerContextFixture>
    {
        private readonly CustomerContext _context;

        public CustomerIntegrationTests(CustomerContextFixture fixture)
        {
            _context = fixture.Context;
        }

        [Fact]
        public async Task Customer_EmailUniqueness_IsEnforced()
        {
            var c1 = new Customer { FirstName = "John", LastName = "Doe", Email = "dup@test.com" };
            var c2 = new Customer { FirstName = "Jane", LastName = "Smith", Email = "dup@test.com" };

            await _context.Customers.AddAsync(c1);
            await _context.SaveChangesAsync();

            await _context.Customers.AddAsync(c2);

            await Assert.ThrowsAsync<DbUpdateException>(() =>
                _context.SaveChangesAsync());
        }
    }
}
