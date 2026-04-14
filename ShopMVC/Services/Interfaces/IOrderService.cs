using ShopMVC.Models;

namespace ShopMVC.Services.Interfaces
{
    public interface IOrderService
    {
        // Create order
        Task<(bool success, string message, int orderId)> CreateOrderAsync(int userId, decimal totalAmount, string shippingAddress);

        // Get order by ID
        Task<Order?> GetOrderByIdAsync(int orderId);

        // Get orders by user
        Task<IEnumerable<Order>> GetOrdersByUserAsync(int userId, string? status = null);

        // Update order status
        Task<(bool success, string message)> UpdateOrderStatusAsync(int orderId, string newStatus, int? approvedBy = null);

        // Admin functions
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
        Task<IEnumerable<OrderDetail>> GetOrderDetailsAsync(int orderId);
        Task<bool> ApproveOrderAsync(int orderId, int staffId);
        Task<bool> RejectOrderAsync(int orderId, int staffId);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
    }
}