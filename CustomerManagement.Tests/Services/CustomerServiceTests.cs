using Xunit;
using Moq;
using FluentAssertions;
using CustomerManagement.Data.Interfaces;
using CustomerManagement.Data.Services.Implementation;
using CustomerManagement.Models;
using CustomerManagement.Models.DTOs;

namespace CustomerManagement.Tests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _mockCustomerRepo;
        private readonly Mock<IStatisticsRepository> _mockStatsRepo;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _mockCustomerRepo = new Mock<ICustomerRepository>();
            _mockStatsRepo = new Mock<IStatisticsRepository>();
            _customerService = new CustomerService(_mockCustomerRepo.Object, _mockStatsRepo.Object);
        }

        [Fact]
        public async Task GetAllActiveCustomersAsync_ReturnsCustomerDtos()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer
                {
                    CustomerId = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@test.com",
                    Phone = "123-456-7890",
                    IsActive = true,
                    CustomerType = "Individual",
                    Addresses = new List<Address>(),
                    CustomerOrders = new List<CustomerOrder>()
                },
                new Customer
                {
                    CustomerId = 2,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane@test.com",
                    Phone = "098-765-4321",
                    IsActive = true,
                    CustomerType = "Premium",
                    Addresses = new List<Address>(),
                    CustomerOrders = new List<CustomerOrder>()
                }
            };

            _mockCustomerRepo.Setup(repo => repo.GetAllActiveAsync())
                .ReturnsAsync(customers);

            // Act
            var result = await _customerService.GetAllActiveCustomersAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].FullName.Should().Be("John Doe");
            result[0].Email.Should().Be("john@test.com");
            result[1].FullName.Should().Be("Jane Smith");
            result[1].CustomerType.Should().Be("Premium");
        }

        [Fact]
        public async Task GetCustomerByIdAsync_WithValidId_ReturnsCustomerDto()
        {
            // Arrange
            var customerId = 1;
            var customer = new Customer
            {
                CustomerId = customerId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Phone = "123-456-7890",
                IsActive = true,
                CustomerType = "Individual",
                Addresses = new List<Address>
                {
                    new Address
                    {
                        AddressId = 1,
                        Street = "123 Main St",
                        City = "Anytown",
                        State = "CA",
                        ZipCode = "12345",
                        AddressType = "Home",
                        IsPrimary = true
                    }
                },
                CustomerOrders = new List<CustomerOrder>()
            };

            _mockCustomerRepo.Setup(repo => repo.GetByIdWithIncludesAsync(customerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            result.Should().NotBeNull();
            result.CustomerId.Should().Be(customerId);
            result.FullName.Should().Be("John Doe");
            result.Addresses.Should().HaveCount(1);
            result.Addresses[0].AddressType.Should().Be("Home");
        }

        [Fact]
        public async Task GetCustomerByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var customerId = 999;
            _mockCustomerRepo.Setup(repo => repo.GetByIdWithIncludesAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateCustomerAsync_WithValidData_ReturnsCustomerDto()
        {
            // Arrange
            var createDto = new CreateCustomerDto
            {
                FirstName = "New",
                LastName = "Customer",
                Email = "new@test.com",
                Phone = "555-123-4567",
                CustomerType = "Individual"
            };

            _mockCustomerRepo.Setup(repo => repo.EmailExistsAsync(createDto.Email, null))
                .ReturnsAsync(false);

            _mockCustomerRepo.Setup(repo => repo.AddAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);

            _mockCustomerRepo.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _customerService.CreateCustomerAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("New Customer");
            result.Email.Should().Be("new@test.com");
            result.IsActive.Should().BeTrue();

            _mockCustomerRepo.Verify(repo => repo.AddAsync(It.IsAny<Customer>()), Times.Once);
            _mockCustomerRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateCustomerAsync_WithDuplicateEmail_ThrowsException()
        {
            // Arrange
            var createDto = new CreateCustomerDto
            {
                FirstName = "Duplicate",
                LastName = "Email",
                Email = "existing@test.com",
                Phone = "555-123-4567",
                CustomerType = "Individual"
            };

            _mockCustomerRepo.Setup(repo => repo.EmailExistsAsync(createDto.Email, null))
                .ReturnsAsync(true);    

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _customerService.CreateCustomerAsync(createDto));

            exception.Message.Should().Contain("already exists");
            _mockCustomerRepo.Verify(repo => repo.AddAsync(It.IsAny<Customer>()), Times.Never);
        }

        [Fact]
        public async Task SearchCustomersAsync_WithValidQuery_ReturnsMatchingCustomers()
        {
            // Arrange
            var searchQuery = "john";
            var matchingCustomers = new List<Customer>
            {
                new Customer
                {
                    CustomerId = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@test.com",
                    IsActive = true,
                    Addresses = new List<Address>(),
                    CustomerOrders = new List<CustomerOrder>()
                }
            };

            _mockCustomerRepo.Setup(repo => repo.SearchAsync(searchQuery))
                .ReturnsAsync(matchingCustomers);

            // Act
            var result = await _customerService.SearchCustomersAsync(searchQuery);

            // Assert
            result.Should().HaveCount(1);
            result[0].FullName.Should().Contain("John");
        }

        [Fact]
        public async Task DeleteCustomerAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            var customerId = 1;
            var customer = new Customer { CustomerId = customerId, IsActive = true };

            _mockCustomerRepo.Setup(repo => repo.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            _mockCustomerRepo.Setup(repo => repo.DeleteAsync(customerId))
                .Returns(Task.CompletedTask);

            _mockCustomerRepo.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _customerService.DeleteCustomerAsync(customerId);

            // Assert
            result.Should().BeTrue();
            _mockCustomerRepo.Verify(repo => repo.DeleteAsync(customerId), Times.Once);
            _mockCustomerRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCustomerAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            var customerId = 999;
            _mockCustomerRepo.Setup(repo => repo.GetByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _customerService.DeleteCustomerAsync(customerId);

            // Assert
            result.Should().BeFalse();
            _mockCustomerRepo.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
