using CustomerManagement.Models.DTOs;

namespace CustomerManagement.Data.Services.Interface
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<List<OrderDto>> GetOrdersByCustomerIdAsync(int customerId);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task<OrderDto> UpdateOrderAsync(int id, CreateOrderDto updateOrderDto);
        Task<bool> DeleteOrderAsync(int id);
    }
}
