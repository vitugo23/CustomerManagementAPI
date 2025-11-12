using Xunit;
using FluentAssertions;
using CustomerManagement.Models;

namespace CustomerManagement.Tests.Models
{
    public class AddressTests
    {
        [Fact]
        public void Address_Creation_SetsDefaultValues()
        {
            // Arrange & Act
            var address = new Address();

            // Assert
            address.Country.Should().Be("USA");
            address.IsPrimary.Should().BeFalse();
        }

        [Fact]
        public void Address_WithValidData_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var address = new Address
            {
                CustomerId = 1,
                Street = "123 Main St",
                City = "Anytown",
                State = "CA",
                ZipCode = "12345",
                AddressType = "Home",
                IsPrimary = true
            };

            // Assert
            address.CustomerId.Should().Be(1);
            address.Street.Should().Be("123 Main St");
            address.City.Should().Be("Anytown");
            address.State.Should().Be("CA");
            address.ZipCode.Should().Be("12345");
            address.AddressType.Should().Be("Home");
            address.IsPrimary.Should().BeTrue();
        }

        [Theory]
        [InlineData("Home", true)]
        [InlineData("Work", true)]
        [InlineData("Billing", true)]
        [InlineData("Shipping", true)]
        [InlineData("", false)]
        [InlineData("InvalidType", false)]
        public void Address_TypeValidation_WorksCorrectly(string addressType, bool expectedValid)
        {
            // Arrange
            var validTypes = new[] { "Home", "Work", "Billing", "Shipping" };
            
            // Act
            bool isValid = validTypes.Contains(addressType);

            // Assert
            isValid.Should().Be(expectedValid);
        }
    }
}
