using ScopeSkyCafeteria.DTOs;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;

namespace ScopeSkyCafeteria.Repositories
{
    public interface IOrderRepository
    {
        // User
        Task<CreateOrderResultDTO> CreateOrderAsync(Order order);

        Task<List<Order>> GetOrdersByUserIdAsync(Guid userId);

        // Admin & SuperAdmin
        Task<List<Order>> GetAllOrdersAsync();

        Task<Order?> AcceptOrderAsync(Guid orderId,Guid adminId);

        Task<Order?> ReadyOrderAsync(Guid orderId);

        Task<Order?> OnTheWayOrderAsync(Guid orderId);

        Task<Order?> DeliverOrderAsync(Guid orderId,Guid adminId);

        // SuperAdmin
        Task<Order?> DeleteOrderAsync(Guid id);

        Task<List<Order>> GetOrdersByStatusAsync(OrderStatus status);
    }
}