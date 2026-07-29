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

        public async Task<OrderItem?> UpdateOrderItemAsync(Guid id, OrderItem orderItem)
        {
            var existingOrderItem = await dbContext.OrderItems
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existingOrderItem == null)
            {
                return null;
            }

            existingOrderItem.OrderId = orderItem.OrderId;
            existingOrderItem.ProductId = orderItem.ProductId;
            existingOrderItem.Quantity = orderItem.Quantity;
            existingOrderItem.Price = orderItem.Price;

            await dbContext.SaveChangesAsync();

            return existingOrderItem;
        }

        public async Task<OrderItem?> DeleteOrderItemAsync(Guid id)
        {
            var existingOrderItem = await dbContext.OrderItems
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existingOrderItem == null)
            {
                return null;
            }

            dbContext.OrderItems.Remove(existingOrderItem);

            await dbContext.SaveChangesAsync();

            return existingOrderItem;
        }
    }
}