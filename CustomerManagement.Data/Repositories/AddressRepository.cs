using Microsoft.EntityFrameworkCore;
using CustomerManagement.Data.Interfaces;
using CustomerManagement.Models;

namespace CustomerManagement.Data.Repositories
{
    // Address repository implementation
    public class AddressRepository : IAddressRepository
    {
        private readonly CustomerContext _context;

        public AddressRepository(CustomerContext context)
        {
            _context = context;
        }

        // Get addresses by customer ID
        public async Task<List<Address>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Addresses
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
        }
        // Get address by ID

        public async Task<Address?> GetByIdAsync(int id)
        {
            return await _context.Addresses.FindAsync(id);
        }
        // Add a new address

        public async Task AddAsync(Address address)
        {
            await _context.Addresses.AddAsync(address);
        }
        // Update an existing address

        public async Task UpdateAsync(Address address)
        {
            _context.Addresses.Update(address);
        }
        // Delete an address by ID

        public async Task DeleteAsync(int id)
        {
            var address = await GetByIdAsync(id);
            if (address != null)
            {
                _context.Addresses.Remove(address);
            }
        }
        // Set an address as primary for a customer

        public async Task SetPrimaryAddressAsync(int customerId, int addressId)
        {
            var addresses = await GetByCustomerIdAsync(customerId);

            foreach (var address in addresses)
            {
                address.IsPrimary = address.AddressId == addressId;
            }
        }
        // Save changes to the database

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
