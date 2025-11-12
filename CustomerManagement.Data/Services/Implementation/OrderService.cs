using CustomerManagement.Data.Interfaces;
using CustomerManagement.Data.Services.Interface;
using CustomerManagement.Models;
using CustomerManagement.Models.DTOs;

namespace CustomerManagement.Data.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        // CRUD Operations for Orders

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToOrderDto).ToList();
        }
        // Get order by ID

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order == null ? null : MapToOrderDto(order);
        }
        // Get orders by Customer ID

        public async Task<List<OrderDto>> GetOrdersByCustomerIdAsync(int customerId)
        {
            var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
            return orders.Select(MapToOrderDto).ToList();
        }
        // Create a new order

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            var order = new Order
            {
                OrderNumber = createOrderDto.OrderNumber,
                TotalAmount = createOrderDto.TotalAmount,
                Status = createOrderDto.Status,
                Description = createOrderDto.Description,
                OrderDate = DateTime.UtcNow
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            // Add customer relationships
            foreach (var customerId in createOrderDto.CustomerIds)
            {
                var customerOrder = new CustomerOrder
                {
                    CustomerId = customerId,
                    OrderId = order.OrderId,
                    Role = "Primary",
                    AssignedDate = DateTime.UtcNow
                };
                await _orderRepository.AddCustomerOrderAsync(customerOrder);
            }

            await _orderRepository.SaveChangesAsync();

            return MapToOrderDto(order);
        }
        // Update an existing order

        public async Task<OrderDto> UpdateOrderAsync(int id, CreateOrderDto updateOrderDto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            order.OrderNumber = updateOrderDto.OrderNumber;
            order.TotalAmount = updateOrderDto.TotalAmount;
            order.Status = updateOrderDto.Status;
            order.Description = updateOrderDto.Description;

            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();

            return MapToOrderDto(order);
        }
        // Delete an order

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return false;

            await _orderRepository.DeleteAsync(id);
            await _orderRepository.SaveChangesAsync();
            return true;
        }
        // Helper method to map Order to OrderDto

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Description = order.Description,
                Customers = order.CustomerOrders.Select(co => new CustomerSummaryDto
                {
                    CustomerId = co.CustomerId,
                    FullName = $"{co.Customer.FirstName} {co.Customer.LastName}",
                    Email = co.Customer.Email,
                    Role = co.Role
                }).ToList()
            };
        }
    }
}