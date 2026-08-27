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

        Task<Order?> AcceptOrderAsync(Guid orderId, Guid adminId);

        Task<Order?> ReadyOrderAsync(Guid orderId);

        // SuperAdmin
        Task<Order?> DeleteOrderAsync(Guid id);

        Task<List<Order>> GetPendingOrdersAsync();

        Task<List<Order>> GetAcceptedOrdersAsync();

        Task<List<Order>> GetReadyOrdersAsync();
    }
}