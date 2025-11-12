using CustomerManagement.Models;

namespace CustomerManagement.Data.Interfaces
{
    //Address repository interface
    public interface IAddressRepository
    {
        Task<List<Address>> GetByCustomerIdAsync(int customerId);
        Task<Address?> GetByIdAsync(int id);
        Task AddAsync(Address address);
        Task UpdateAsync(Address address);
        Task DeleteAsync(int id);
        Task SetPrimaryAddressAsync(int customerId, int addressId);
        Task SaveChangesAsync();
    }
}