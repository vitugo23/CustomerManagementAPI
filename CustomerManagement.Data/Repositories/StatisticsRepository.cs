using Microsoft.EntityFrameworkCore;
using CustomerManagement.Data.Interfaces;
using CustomerManagement.Models.DTOs;

namespace CustomerManagement.Data.Repositories
{
    public class StatisticsRepository : IStatisticsRepository
    {
        private readonly CustomerContext _context;

        public StatisticsRepository(CustomerContext context)
        {
            _context = context;
        }

        public async Task<CustomerStatsDto> GetCustomerStatisticsAsync()
        {
            return new CustomerStatsDto
            {
                TotalCustomers = await GetTotalCustomersAsync(),
                ActiveCustomers = await GetActiveCustomersAsync(),
                InactiveCustomers = await GetTotalCustomersAsync() - await GetActiveCustomersAsync(),
                TotalOrders = await GetTotalOrdersAsync(),
                TotalRevenue = await GetTotalRevenueAsync(),
                CustomerTypeBreakdown = await GetCustomerTypeBreakdownAsync()
            };
        }
        // Helper methods to fetch individual statistics
        public async Task<int> GetTotalCustomersAsync()
        {
            return await _context.Customers.CountAsync();
        }
        // Helper method to get active customers count
        public async Task<int> GetActiveCustomersAsync()
        {
            return await _context.Customers.CountAsync(c => c.IsActive);
        }
        // Helper method to get total orders count

        public async Task<int> GetTotalOrdersAsync()
        {
            return await _context.Orders.CountAsync();
        }
        // Helper method to get total revenue
        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders.SumAsync(o => o.TotalAmount);
        }
        // Helper method to get customer type breakdown

        public async Task<List<CustomerTypeStatsDto>> GetCustomerTypeBreakdownAsync()
        {
            return await _context.Customers
                .Where(c => c.IsActive)
                .GroupBy(c => c.CustomerType)
                .Select(g => new CustomerTypeStatsDto
                {
                    CustomerType = g.Key ?? "Unknown",
                    Count = g.Count(),
                    TotalRevenue = 0 // Can be calculated if needed
                })
                .ToListAsync();
        }
    }
}