using Microsoft.EntityFrameworkCore;
using ScopeSkyCafeteria.Data;
using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Repositories
{
    public class SQLOrderRepository : IOrderRepository
    {
        private readonly SSCafeteriaDbContext dbContext;

        public SQLOrderRepository(SSCafeteriaDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            await dbContext.Orders.AddAsync(order);

            await dbContext.SaveChangesAsync();

            return order;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await dbContext.Orders.Include(o => o.User).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ToListAsync();
        }


        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            return await dbContext.Orders.Include(o => o.User).Include(o => o.OrderItems).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> UpdateOrderAsync(Guid id, Order order)
        {
            var existingOrder = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if (existingOrder == null)
            {
                return null;
            }

            existingOrder.Status = order.Status;

            existingOrder.TotalPrice = order.TotalPrice;

            await dbContext.SaveChangesAsync();

            return existingOrder;
        }


        public async Task<Order?> DeleteOrderAsync(Guid id)
        {
            var existingOrder = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (existingOrder == null)
            {
                return null;
            }

            dbContext.Orders.Remove(existingOrder);

            await dbContext.SaveChangesAsync();

            return existingOrder;
        }
    }
}