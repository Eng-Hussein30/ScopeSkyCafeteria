using Microsoft.EntityFrameworkCore;
using ScopeSkyCafeteria.Data;
using ScopeSkyCafeteria.DTOs;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;

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

        public async Task<CreateOrderResultDTO> CreateOrderAsync(Order order)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();

            CreateOrderResultDTO result = null!;

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await dbContext.Database.BeginTransactionAsync();

                try
                {
                    // =====================================================
                    // Variables
                    // =====================================================

                    decimal totalPrice = 0;

                    decimal paidFromWallet = 0;
                    decimal addedToDebt = 0;

                    // القيم قبل الخصم
                    decimal walletBalanceBefore = 0;
                    decimal debtBefore = 0;

                    // القيم بعد الخصم
                    decimal remainingBalance = 0;
                    decimal currentDebt = 0;

                    // =====================================================
                    // Validate Products & Calculate Total Price
                    // =====================================================

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

                        // سعر المنتج الحقيقي من قاعدة البيانات
                        item.Price = product.Price;

                        // حتى يظهر اسم المنتج في الـ Response
                        item.Product = product;

                        // حساب مجموع الطلب
                        totalPrice += product.Price * item.Quantity;
                    }

                    // =====================================================
                    // Set Order Data
                    // =====================================================

                    order.TotalPrice = totalPrice;
                    order.Status = OrderStatus.Pending;

                    if (order.Id == Guid.Empty)
                    {
                        order.Id = Guid.NewGuid();
                    }

                    // =====================================================
                    // Get User Wallet
                    // =====================================================

                    var wallet = await dbContext.Wallets
                        .FirstOrDefaultAsync(w => w.UserId == order.UserId);

                    if (wallet == null)
                    {
                        throw new InvalidOperationException(
                            "User wallet was not found.");
                    }

                    // =====================================================
                    // Save Wallet Values BEFORE Payment
                    // =====================================================

                    walletBalanceBefore = wallet.Balance;
                    debtBefore = wallet.Debt;

                    // =====================================================
                    // Wallet Payment
                    // =====================================================

                    if (order.PaymentMethod == PaymentMethod.Wallet)
                    {
                        // =================================================
                        // Calculate Amount Paid From Wallet
                        // =================================================

                        paidFromWallet = Math.Min(
                            walletBalanceBefore,
                            totalPrice);

                        // =================================================
                        // Calculate Remaining Amount As Debt
                        // =================================================

                        addedToDebt = totalPrice - paidFromWallet;

                        // =================================================
                        // Calculate NEW Wallet Balance
                        // =================================================

                        remainingBalance =
                            walletBalanceBefore - paidFromWallet;

                        // =================================================
                        // Calculate NEW Debt
                        // =================================================

                        currentDebt =
                            debtBefore + addedToDebt;

                        // =================================================
                        // Update Wallet
                        // =================================================

                        wallet.Balance = remainingBalance;
                        wallet.Debt = currentDebt;
                        wallet.UpdatedAt = DateTime.UtcNow;

                        // =================================================
                        // Wallet Purchase Transaction
                        // =================================================

                        if (paidFromWallet > 0)
                        {
                            var purchaseTransaction = new WalletTransaction
                            {
                                Id = Guid.NewGuid(),
                                WalletId = wallet.Id,
                                Amount = paidFromWallet,
                                Type = WalletTransactionType.Purchase,
                                OrderId = order.Id,
                                PerformedByUserId = order.UserId,
                                Note =
                                    $"Payment for order #{order.Id} from wallet balance",
                                CreatedAt = DateTime.UtcNow
                            };

                            await dbContext.WalletTransactions.AddAsync(
                                purchaseTransaction);
                        }

                        // =================================================
                        // Debt Transaction
                        // =================================================

                        if (addedToDebt > 0)
                        {
                            var debtTransaction = new WalletTransaction
                            {
                                Id = Guid.NewGuid(),
                                WalletId = wallet.Id,
                                Amount = addedToDebt,
                                Type = WalletTransactionType.Debt,
                                OrderId = order.Id,
                                PerformedByUserId = order.UserId,
                                Note =
                                    $"Remaining amount of order #{order.Id} added to debt",
                                CreatedAt = DateTime.UtcNow
                            };

                            await dbContext.WalletTransactions.AddAsync(
                                debtTransaction);
                        }
                    }

                    // =====================================================
                    // Cash Payment
                    // =====================================================

                    else if (order.PaymentMethod == PaymentMethod.Cash)
                    {
                        // الكاش لا يخصم من المحفظة
                        // ولا يضيف دين

                        paidFromWallet = 0;
                        addedToDebt = 0;

                        // تبقى المحفظة والدين كما هما
                        remainingBalance = walletBalanceBefore;
                        currentDebt = debtBefore;
                    }

                    // =====================================================
                    // Invalid Payment Method
                    // =====================================================

                    else
                    {
                        throw new InvalidOperationException(
                            "Invalid payment method.");
                    }

                    // =====================================================
                    // Add Order
                    // =====================================================

                    await dbContext.Orders.AddAsync(order);

                    // =====================================================
                    // Save Everything
                    // =====================================================

                    await dbContext.SaveChangesAsync();

                    // =====================================================
                    // Commit Transaction
                    // =====================================================

                    await transaction.CommitAsync();

                    // =====================================================
                    // Create Result
                    // =====================================================

                    result = new CreateOrderResultDTO
                    {
                        Order = order,

                        // ==============================
                        // BEFORE
                        // ==============================

                        WalletBalanceBefore = walletBalanceBefore,
                        DebtBefore = debtBefore,

                        // ==============================
                        // PAYMENT
                        // ==============================

                        PaidFromWallet = paidFromWallet,
                        AddedToDebt = addedToDebt,

                        // ==============================
                        // AFTER
                        // ==============================

                        RemainingBalance = remainingBalance,
                        CurrentDebt = currentDebt
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return result;
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
        // Deliver Order
        // =====================================================

        public async Task<Order?> DeliverOrderAsync(
            Guid orderId,
            Guid adminId)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            if (order.Status != OrderStatus.OnTheWay)
            {
                throw new InvalidOperationException(
                    "Only orders that are on the way can be delivered.");
            }

            order.DeliveredByAdminId = adminId;
            order.DeliveredAt = DateTime.UtcNow;
            order.Status = OrderStatus.Delivered;

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
            return await GetOrdersQuery().ToListAsync();
        }

        // =====================================================
        // Get My Orders
        // =====================================================

        public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            return await GetOrdersQuery()
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        // =====================================================
        // Get Orders By Status
        // =====================================================

        public async Task<List<Order>> GetOrdersByStatusAsync(
            OrderStatus status)
        {
            return await GetOrdersQuery()
                .Where(o => o.Status == status)
                .ToListAsync();
        }

        // =====================================================
        // Get Orders Query
        // =====================================================

        private IQueryable<Order> GetOrdersQuery()
        {
            return dbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Admin)
                .Include(o => o.DeliveredByAdmin)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product);
        }

        // =====================================================
        // On The Way Order
        // =====================================================

        public async Task<Order?> OnTheWayOrderAsync(Guid orderId)
        {
            var order = await dbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            if (order.Status != OrderStatus.Ready)
            {
                throw new InvalidOperationException(
                    "Only ready orders can be marked as on the way.");
            }

            order.Status = OrderStatus.OnTheWay;

            await dbContext.SaveChangesAsync();

            return order;
        }
    }
}
