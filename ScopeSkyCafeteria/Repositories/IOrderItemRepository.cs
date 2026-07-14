using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public interface IOrderItemRepository
    {
        Task<OrderItem> CreateOrderItemAsync(OrderItem orderItem);
        Task<OrderItem?> GetOrderItemByIdAsync(Guid id);
        Task<List<OrderItem>> GetAllOrderItemsAsync();
        Task<OrderItem?> UpdateOrderItemAsync(Guid id, OrderItem orderItem);
        Task<OrderItem?> DeleteOrderItemAsync(Guid id);
    }
}
