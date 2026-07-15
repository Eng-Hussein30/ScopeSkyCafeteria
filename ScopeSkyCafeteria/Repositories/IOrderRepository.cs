using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order);

        Task<List<Order>> GetAllOrdersAsync();

        Task<List<Order>> GetOrdersByUserIdAsync(Guid userId);

        Task<Order?> GetOrderByIdAsync(Guid id);

        Task<Order?> AcceptOrderAsync(Guid orderId, Guid adminId);

        Task<Order?> ChangeStatusAsync(Guid orderId, OrderStatus status);

        Task<Order?> UpdateOrderAsync(Guid id, Order order);

        Task<Order?> DeleteOrderAsync(Guid id);
    }
}