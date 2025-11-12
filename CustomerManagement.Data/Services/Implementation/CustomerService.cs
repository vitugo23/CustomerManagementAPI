using CustomerManagement.Data.Interfaces;
using CustomerManagement.Data.Services.Interface;
using CustomerManagement.Models;
using CustomerManagement.Models.DTOs;

namespace CustomerManagement.Data.Services.Implementation
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IStatisticsRepository _statisticsRepository;

        public CustomerService(ICustomerRepository customerRepository, IStatisticsRepository statisticsRepository)
        {
            _customerRepository = customerRepository;
            _statisticsRepository = statisticsRepository;
        }
        // Get all active customers with their primary address and order count

        public async Task<List<CustomerDto>> GetAllActiveCustomersAsync()
        {
            var customers = await _customerRepository.GetAllActiveAsync();
            return customers.Select(MapToCustomerDto).ToList();
        }
        // Get customer by ID with detailed information
        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdWithIncludesAsync(id);
            return customer == null ? null : MapToCustomerDto(customer);
        }
        // Search customers by name or email

        public async Task<List<CustomerDto>> SearchCustomersAsync(string query)
        {
            var customers = await _customerRepository.SearchAsync(query);
            return customers.Select(MapToCustomerDto).ToList();
        }
        // Filter customers by type (e.g., Individual, Business)

        public async Task<List<CustomerDto>> FilterCustomersByTypeAsync(string customerType)
        {
            var customers = await _customerRepository.FilterByTypeAsync(customerType);
            return customers.Select(MapToCustomerDto).ToList();
        }
        // Create a new customer with validation for unique email

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto)
        {
            // Validate email uniqueness
            if (await _customerRepository.EmailExistsAsync(createCustomerDto.Email))
            {
                throw new InvalidOperationException($"A customer with email '{createCustomerDto.Email}' already exists.");
            }

            var customer = new Customer
            {
                FirstName = createCustomerDto.FirstName,
                LastName = createCustomerDto.LastName,
                Email = createCustomerDto.Email,
                Phone = createCustomerDto.Phone,
                CustomerType = createCustomerDto.CustomerType,
                Notes = createCustomerDto.Notes,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            // Add primary address if provided
            if (createCustomerDto.PrimaryAddress != null)
            {
                customer.Addresses.Add(new Address
                {
                    AddressType = createCustomerDto.PrimaryAddress.AddressType,
                    Street = createCustomerDto.PrimaryAddress.Street,
                    City = createCustomerDto.PrimaryAddress.City,
                    State = createCustomerDto.PrimaryAddress.State,
                    ZipCode = createCustomerDto.PrimaryAddress.ZipCode,
                    Country = createCustomerDto.PrimaryAddress.Country,
                    IsPrimary = createCustomerDto.PrimaryAddress.IsPrimary
                });
            }

            await _customerRepository.AddAsync(customer);
            await _customerRepository.SaveChangesAsync();

            return MapToCustomerDto(customer);
        }
        // Update existing customer details with validation for unique email

        public async Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto)
        {
            var customer = await _customerRepository.GetByIdWithIncludesAsync(id);
            if (customer == null)
                throw new InvalidOperationException("Customer not found");

            // Validate email uniqueness (excluding current customer)
            if (await _customerRepository.EmailExistsAsync(updateCustomerDto.Email, id))
            {
                throw new InvalidOperationException($"A customer with email '{updateCustomerDto.Email}' already exists.");
            }

            customer.FirstName = updateCustomerDto.FirstName;
            customer.LastName = updateCustomerDto.LastName;
            customer.Email = updateCustomerDto.Email;
            customer.Phone = updateCustomerDto.Phone;
            customer.IsActive = updateCustomerDto.IsActive;
            customer.CustomerType = updateCustomerDto.CustomerType;
            customer.Notes = updateCustomerDto.Notes;

            await _customerRepository.UpdateAsync(customer);
            await _customerRepository.SaveChangesAsync();

            return MapToCustomerDto(customer);
        }
        // Delete customer by ID with check for existing orders

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return false;

            await _customerRepository.DeleteAsync(id);
            await _customerRepository.SaveChangesAsync();
            return true;
        }
        // Get customer statistics like total customers, active customers.

        public async Task<CustomerStatsDto> GetCustomerStatisticsAsync()
        {
            return await _statisticsRepository.GetCustomerStatisticsAsync();
        }
        // Helper method to map Customer entity to CustomerDto

        private CustomerDto MapToCustomerDto(Customer customer)
        {
            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                FullName = $"{customer.FirstName} {customer.LastName}",
                Email = customer.Email,
                Phone = customer.Phone,
                CreatedDate = customer.CreatedDate,
                IsActive = customer.IsActive,
                CustomerType = customer.CustomerType ?? "Individual",
                Addresses = customer.Addresses.Select(a => new AddressDto
                {
                    AddressId = a.AddressId,
                    AddressType = a.AddressType,
                    FullAddress = $"{a.Street}, {a.City}, {a.State} {a.ZipCode}",
                    IsPrimary = a.IsPrimary
                }).ToList(),
                TotalOrders = customer.CustomerOrders.Count,
                TotalSpent = 0
            };
        }
    }
}