using Microsoft.EntityFrameworkCore;
using CustomerManagement.Data.Interfaces;
using CustomerManagement.Models;

namespace CustomerManagement.Data.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerContext _context;

        public CustomerRepository(CustomerContext context)
        {
            _context = context;
        }
        // Retrieves all active customers with their addresses and orders

        public async Task<List<Customer>> GetAllActiveAsync()
        {
            return await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.CustomerOrders)
                .Where(c => c.IsActive)
                .ToListAsync();
        }
        // Retrieves a customer by ID without related entities

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        // Retrieves a customer by ID with related addresses and orders

        public async Task<Customer?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.CustomerOrders)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
        }
        // Searches active customers by first name, last name, or email
        public async Task<List<Customer>> SearchAsync(string query)
        {
            return await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.CustomerOrders)
                .Where(c => c.IsActive && 
                           (c.FirstName.Contains(query) || 
                            c.LastName.Contains(query) || 
                            c.Email.Contains(query)))
                .ToListAsync();
        }
        // Filters active customers by customer type

        public async Task<List<Customer>> FilterByTypeAsync(string customerType)
        {
            return await _context.Customers
                .Include(c => c.Addresses)
                .Include(c => c.CustomerOrders)
                .Where(c => c.IsActive && c.CustomerType == customerType)
                .ToListAsync();
        }
        // Retrieves a customer by email

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);
        }
        // Adds a new customer

        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
        }
        // Updates an existing customer

        public async Task UpdateAsync(Customer customer)
        {
            customer.LastUpdated = DateTime.UtcNow;
            _context.Customers.Update(customer);
        }
        // Soft deletes a customer by setting IsActive to false

        public async Task DeleteAsync(int id)
        {
            var customer = await GetByIdAsync(id);
            if (customer != null)
            {
                customer.IsActive = false;
                customer.LastUpdated = DateTime.UtcNow;
            }
        }
        // Checks if an email already exists, excluding a specific customer ID if provided
        public async Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null)
        {
            var query = _context.Customers.Where(c => c.Email == email);
            
            if (excludeCustomerId.HasValue)
            {
                query = query.Where(c => c.CustomerId != excludeCustomerId.Value);
            }
            
            return await query.AnyAsync();
        }
        // Saves changes to the database

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}