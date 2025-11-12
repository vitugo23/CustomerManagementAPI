using CustomerManagement.Models;
using CustomerManagement.Models.DTOs;

namespace CustomerManagement.Data.Services.Interface
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetAllActiveCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<List<CustomerDto>> SearchCustomersAsync(string query);
        Task<List<CustomerDto>> FilterCustomersByTypeAsync(string customerType);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
        Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto);
        Task<bool> DeleteCustomerAsync(int id);
        Task<CustomerStatsDto> GetCustomerStatisticsAsync();
    }
}