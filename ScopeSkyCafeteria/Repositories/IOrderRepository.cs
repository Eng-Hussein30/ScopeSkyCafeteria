using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order);

        Task<List<Order>> GetAllOrdersAsync();

        Task<Order?> GetOrderByIdAsync(Guid id);

        Task<Order?> UpdateOrderAsync(Guid id, Order order);

        Task<Order?> DeleteOrderAsync(Guid id);
    }
}