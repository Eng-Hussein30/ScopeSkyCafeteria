using ScopeSkyCafeteria.Data;
using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public class SQLOrderItemRepository : IOrderItemRepository
    {
        private readonly SSCafeteriaDbContext dbContext;
        public SQLOrderItemRepository(SSCafeteriaDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<OrderItem> CreateOrderItemAsync(OrderItem orderItem)
        {
           await dbContext.OrderItems.AddAsync(orderItem);
            await dbContext.SaveChangesAsync();
            return orderItem;
        }

        public Task<OrderItem?> DeleteOrderItemAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderItem>> GetAllOrderItemsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<OrderItem?> GetOrderItemByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<OrderItem?> UpdateOrderItemAsync(Guid id, OrderItem orderItem)
        {
            throw new NotImplementedException();
        }
    }
}
