using CustomerManagement.Models.DTOs;

namespace CustomerManagement.Data.Interfaces
{
    // Statistics repository interface
    public interface IStatisticsRepository
    {
        Task<CustomerStatsDto> GetCustomerStatisticsAsync();
        Task<int> GetTotalCustomersAsync();
        Task<int> GetActiveCustomersAsync();
        Task<int> GetTotalOrdersAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<List<CustomerTypeStatsDto>> GetCustomerTypeBreakdownAsync();
    }
}