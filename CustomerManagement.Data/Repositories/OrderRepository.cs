using Microsoft.EntityFrameworkCore;
using CustomerManagement.Data.Interfaces;
using CustomerManagement.Models;

namespace CustomerManagement.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly CustomerContext _context;

        public OrderRepository(CustomerContext context)
        {
            _context = context;
        }
        // CRUD operations for Orders

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.CustomerOrders)
                .ThenInclude(co => co.Customer)
                .ToListAsync();
        }
        // Get order by ID including associated customers

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.CustomerOrders)
                .ThenInclude(co => co.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }
        // Get orders by Customer ID

        public async Task<List<Order>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.CustomerOrders)
                .Where(o => o.CustomerOrders.Any(co => co.CustomerId == customerId))
                .ToListAsync();
        }
        // Add, Update, Delete operations
        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }
        // Update an existing order
        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
        }
        // Delete an order by ID

        public async Task DeleteAsync(int id)
        {
            var order = await GetByIdAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
        }
        // Add a new CustomerOrder association

        public async Task AddCustomerOrderAsync(CustomerOrder customerOrder)
        {
            await _context.CustomerOrders.AddAsync(customerOrder);
        }
        // Save changes to the database

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
