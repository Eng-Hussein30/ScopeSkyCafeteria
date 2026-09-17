using Microsoft.EntityFrameworkCore;
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

        public async Task<List<OrderItem>> GetAllOrderItemsAsync()
        {
            return await dbContext.OrderItems
                .Include(x => x.Product)
                .Include(x => x.Order)
                .ToListAsync();
        }

        public async Task<OrderItem?> GetOrderItemByIdAsync(Guid id)
        {
            return await dbContext.OrderItems
                .Include(x => x.Product)
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}