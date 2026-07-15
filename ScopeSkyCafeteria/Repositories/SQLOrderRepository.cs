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

        public async Task<Order?> AcceptOrderAsync(Guid orderId, Guid adminId)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            order.AdminId = adminId;
            order.Status = OrderStatus.Accepted;

            await dbContext.SaveChangesAsync();

            return order;
        }

        public async Task<Order?> ChangeStatusAsync(Guid orderId, OrderStatus status)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            order.Status = status;

            await dbContext.SaveChangesAsync();

            return order;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            await dbContext.Orders.AddAsync(order);

            await dbContext.SaveChangesAsync();

            return order;
        }

        public async Task<Order?> DeleteOrderAsync(Guid id)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return null;
            }

            dbContext.Orders.Remove(order);

            await dbContext.SaveChangesAsync();

            return order;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            return await dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            return await dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task<Order?> UpdateOrderAsync(Guid id, Order order)
        {
            var existingOrder = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (existingOrder == null)
            {
                return null;
            }

            existingOrder.Status = order.Status;

            await dbContext.SaveChangesAsync();

            return existingOrder;
        }
    }
}