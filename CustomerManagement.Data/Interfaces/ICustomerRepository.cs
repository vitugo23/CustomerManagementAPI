using CustomerManagement.Models;

namespace CustomerManagement.Data.Interfaces
{
    //customer repository interface
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllActiveAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByIdWithIncludesAsync(int id);
        Task<List<Customer>> SearchAsync(string query);
        Task<List<Customer>> FilterByTypeAsync(string customerType);
        Task<Customer?> GetByEmailAsync(string email);
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null);
        Task SaveChangesAsync();
    }
}