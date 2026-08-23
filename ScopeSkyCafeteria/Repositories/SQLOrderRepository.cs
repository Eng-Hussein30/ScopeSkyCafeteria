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

        // =====================================================
        // Create Order
        // =====================================================

        public async Task<Order> CreateOrderAsync(Order order)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();

            Order createdOrder = null!;

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await dbContext.Database.BeginTransactionAsync();

                try
                {
                    decimal totalPrice = 0;

                    // =================================================
                    // Validate Products
                    // =================================================

                    foreach (var item in order.OrderItems)
                    {
                        if (item.Quantity <= 0)
                        {
                            throw new InvalidOperationException(
                                "Quantity must be greater than zero.");
                        }

                        var product = await dbContext.Products
                            .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                        if (product == null)
                        {
                            throw new InvalidOperationException(
                                $"Product with Id {item.ProductId} was not found.");
                        }

                        if (!product.IsAvailable)
                        {
                            throw new InvalidOperationException(
                                $"Product '{product.Name}' is unavailable.");
                        }

                        // استخدام السعر الحالي للمنتج
                        item.Price = product.Price;

                        // ربط المنتج
                        item.Product = product;

                        // حساب السعر الإجمالي
                        totalPrice += product.Price * item.Quantity;
                    }

                    // =================================================
                    // Set Order Data
                    // =================================================

                    order.TotalPrice = totalPrice;
                    order.Status = OrderStatus.Pending;

                    if (order.Id == Guid.Empty)
                    {
                        order.Id = Guid.NewGuid();
                    }

                    // =================================================
                    // Get User Wallet
                    // =================================================

                    var wallet = await dbContext.Wallets
                        .FirstOrDefaultAsync(w => w.UserId == order.UserId);

                    if (wallet == null)
                    {
                        throw new InvalidOperationException(
                            "User wallet was not found.");
                    }

                    // =================================================
                    // Calculate Payment
                    // =================================================

                    decimal amountFromBalance =
                        Math.Min(wallet.Balance, order.TotalPrice);

                    decimal amountToDebt =
                        order.TotalPrice - amountFromBalance;

                    // =================================================
                    // Deduct Balance
                    // =================================================

                    if (amountFromBalance > 0)
                    {
                        wallet.Balance -= amountFromBalance;
                    }

                    // =================================================
                    // Add Remaining Amount To Debt
                    // =================================================

                    if (amountToDebt > 0)
                    {
                        wallet.Debt += amountToDebt;
                    }

                    wallet.UpdatedAt = DateTime.UtcNow;

                    // =================================================
                    // Add Order
                    // =================================================

                    await dbContext.Orders.AddAsync(order);

                    // =================================================
                    // Wallet Transaction - Purchase
                    // =================================================

                    if (amountFromBalance > 0)
                    {
                        var purchaseTransaction = new WalletTransaction
                        {
                            Id = Guid.NewGuid(),
                            WalletId = wallet.Id,
                            Amount = amountFromBalance,
                            Type = WalletTransactionType.Purchase,
                            OrderId = order.Id,
                            PerformedByUserId = order.UserId,
                            Note = "Payment for order from wallet balance",
                            CreatedAt = DateTime.UtcNow
                        };

                        await dbContext.WalletTransactions.AddAsync(
                            purchaseTransaction);
                    }

                    // =================================================
                    // Wallet Transaction - Debt
                    // =================================================

                    if (amountToDebt > 0)
                    {
                        var debtTransaction = new WalletTransaction
                        {
                            Id = Guid.NewGuid(),
                            WalletId = wallet.Id,
                            Amount = amountToDebt,
                            Type = WalletTransactionType.Debt,
                            OrderId = order.Id,
                            PerformedByUserId = order.UserId,
                            Note = "Remaining order amount added to debt",
                            CreatedAt = DateTime.UtcNow
                        };

                        await dbContext.WalletTransactions.AddAsync(
                            debtTransaction);
                    }

                    // =================================================
                    // Save Everything
                    // =================================================

                    await dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    createdOrder = order;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return createdOrder;
        }

        // =====================================================
        // Accept Order
        // =====================================================

        public async Task<Order?> AcceptOrderAsync(
            Guid orderId,
            Guid adminId)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            if (order.Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException(
                    $"Cannot accept an order with status '{order.Status}'.");
            }

            order.AdminId = adminId;
            order.Status = OrderStatus.Accepted;

            await dbContext.SaveChangesAsync();

            return order;
        }

        // =====================================================
        // Ready Order
        // =====================================================

        public async Task<Order?> ReadyOrderAsync(Guid orderId)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            if (order.Status != OrderStatus.Accepted)
            {
                throw new InvalidOperationException(
                    "Only accepted orders can be marked as ready.");
            }

            order.Status = OrderStatus.Ready;

            await dbContext.SaveChangesAsync();

            return order;
        }

        // =====================================================
        // Delete Order
        // =====================================================

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

        // =====================================================
        // Get All Orders
        // =====================================================

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Admin)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();
        }

        // =====================================================
        // Get My Orders
        // =====================================================

        public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            return await dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Admin)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        // =====================================================
        // Get Pending Orders
        // =====================================================

        public async Task<List<Order>> GetPendingOrdersAsync()
        {
            return await dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Admin)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(o => o.Status == OrderStatus.Pending)
                .ToListAsync();
        }

        // =====================================================
        // Get Accepted Orders
        // =====================================================

        public async Task<List<Order>> GetAcceptedOrdersAsync()
        {
            return await dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Admin)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(o => o.Status == OrderStatus.Accepted)
                .ToListAsync();
        }

        // =====================================================
        // Get Ready Orders
        // =====================================================

        public async Task<List<Order>> GetReadyOrdersAsync()
        {
            return await dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Admin)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(o => o.Status == OrderStatus.Ready)
                .ToListAsync();
        }
    }
}   //  eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiVXNlciIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiOGY5Mjc3YjgtMzVmYS00N2U0LWU1N2EtMDhkZWUxODNmNWU2IiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoidXNlckB0ZXN0LmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlVzZXIiLCJleHAiOjE3ODc0OTg0NTYsImlzcyI6IlNjb3BlU2t5Q2FmZXRlcmlhIiwiYXVkIjoiU2NvcGVTa3lDYWZldGVyaWFVc2VycyJ9.E9FJ64ciTZikm9bN_FCz8wvM7VtCjNt-kUsKE2xA6po                                                     ///////////////////////////////////////////////////////////////////////////////////////////////////////////////           eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQWRtaW4iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImZiNDNiM2UyLTMwMGQtNGQ2My1lNTc5LTA4ZGVlMTgzZjVlNiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQHRlc3QuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE3ODc0OTg0OTgsImlzcyI6IlNjb3BlU2t5Q2FmZXRlcmlhIiwiYXVkIjoiU2NvcGVTa3lDYWZldGVyaWFVc2VycyJ9.TlLA_ADgC8aAZSUPjF8ESCuTp0QUY-xVRgqhks8Ke8s