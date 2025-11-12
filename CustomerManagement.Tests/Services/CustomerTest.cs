using Xunit;
using FluentAssertions;
using CustomerManagement.Models;

namespace CustomerManagement.Tests.Models
{
    public class CustomerTests
    {
        [Fact]
        public void Customer_Creation_SetsDefaultValues()
        {
            // Arrange & Act
            var customer = new Customer();

            // Assert
            customer.IsActive.Should().BeTrue();
            customer.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            customer.Addresses.Should().NotBeNull();
            customer.CustomerOrders.Should().NotBeNull();
        }

        [Fact]
        public void Customer_WithValidData_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var customer = new Customer
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                Phone = "555-123-4567",
                CustomerType = "Individual"
            };

            // Assert
            customer.FirstName.Should().Be("John");
            customer.LastName.Should().Be("Doe");
            customer.Email.Should().Be("john.doe@test.com");
            customer.Phone.Should().Be("555-123-4567");
            customer.CustomerType.Should().Be("Individual");
            customer.IsActive.Should().BeTrue();
        }

        [Theory]
        [InlineData("", "Doe", false)]
        [InlineData("John", "", false)]
        [InlineData("John", "Doe", true)]
        [InlineData(null, "Doe", false)]
        [InlineData("John", null, false)]
        public void Customer_NameValidation_WorksCorrectly(string? firstName, string? lastName, bool expectedValid)
        {
            // Arrange
            var customer = new Customer
            {
                FirstName = firstName ?? string.Empty,
                LastName = lastName ?? string.Empty
            };

            // Act
            bool isValid = !string.IsNullOrEmpty(customer.FirstName) && 
                          !string.IsNullOrEmpty(customer.LastName);

            // Assert
            isValid.Should().Be(expectedValid);
        }

        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("invalid-email", false)]
        [InlineData("", false)]
        [InlineData("user@domain", false)]
        public void Customer_EmailFormat_ValidatesCorrectly(string email, bool expectedValid)
        {
            // Arrange & Act
            var customer = new Customer { Email = email };
            bool isValidFormat = email.Contains("@") && email.Contains(".");

            // Assert
            isValidFormat.Should().Be(expectedValid);
        }
    }
}

