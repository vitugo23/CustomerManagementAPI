using Xunit;
using Moq;
using FluentAssertions;
using CustomerManagement.Data.Interfaces;
using CustomerManagement.Data.Services.Implementation;
using CustomerManagement.Models;
using CustomerManagement.Models.DTOs;

namespace CustomerManagement.Tests.Services
{
    public class AddressServiceTests
    {
        private readonly Mock<IAddressRepository> _mockAddressRepo;
        private readonly Mock<ICustomerRepository> _mockCustomerRepo;
        private readonly AddressService _addressService;

        public AddressServiceTests()
        {
            _mockAddressRepo = new Mock<IAddressRepository>();
            _mockCustomerRepo = new Mock<ICustomerRepository>();
            _addressService = new AddressService(_mockAddressRepo.Object, _mockCustomerRepo.Object);
        }

        [Fact]
        public async Task GetCustomerAddressesAsync_WithValidCustomerId_ReturnsAddresses()
        {
            // Arrange
            var customerId = 1;
            var addresses = new List<Address>
            {
                new Address
                {
                    AddressId = 1,
                    CustomerId = customerId,
                    Street = "123 Main St",
                    City = "Anytown",
                    State = "CA",
                    ZipCode = "12345",
                    AddressType = "Home",
                    IsPrimary = true
                },
                new Address
                {
                    AddressId = 2,
                    CustomerId = customerId,
                    Street = "456 Work Ave",
                    City = "Business City",
                    State = "NY",
                    ZipCode = "67890",
                    AddressType = "Work",
                    IsPrimary = false
                }
            };

            _mockAddressRepo.Setup(repo => repo.GetByCustomerIdAsync(customerId))
                .ReturnsAsync(addresses);

            // Act
            var result = await _addressService.GetCustomerAddressesAsync(customerId);

            // Assert
            result.Should().HaveCount(2);
            result[0].AddressType.Should().Be("Home");
            result[0].IsPrimary.Should().BeTrue();
            result[1].AddressType.Should().Be("Work");
            result[1].IsPrimary.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAddressAsync_WithValidData_ReturnsAddressDto()
        {
            // Arrange
            var customerId = 1;
            var createDto = new CreateAddressDto
            {
                Street = "789 New St",
                City = "New City",
                State = "TX",
                ZipCode = "54321",
                AddressType = "Home",
                IsPrimary = true
            };

            var customer = new Customer { CustomerId = customerId };

            _mockCustomerRepo.Setup(repo => repo.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _mockAddressRepo.Setup(repo => repo.AddAsync(It.IsAny<Address>()))
                .Returns(Task.CompletedTask);

            _mockAddressRepo.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _addressService.CreateAddressAsync(customerId, createDto);

            // Assert
            result.Should().NotBeNull();
            result.AddressType.Should().Be("Home");
            result.IsPrimary.Should().BeTrue();
            result.FullAddress.Should().Contain("789 New St");

            _mockAddressRepo.Verify(repo => repo.AddAsync(It.IsAny<Address>()), Times.Once);
            _mockAddressRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAddressAsync_WithInvalidCustomerId_ThrowsException()
        {
            // Arrange
            var customerId = 999;
            var createDto = new CreateAddressDto
            {
                Street = "789 New St",
                City = "New City",
                State = "TX",
                ZipCode = "54321",
                AddressType = "Home"
            };

            _mockCustomerRepo.Setup(repo => repo.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _addressService.CreateAddressAsync(customerId, createDto));

            exception.Message.Should().Be("Customer not found");
        }
    }
}

