using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScopeSkyCafeteria.DTOs;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;
using ScopeSkyCafeteria.Repositories;
using System.Security.Claims;
using ScopeSkyCafeteria.Services.Interfaces;

namespace ScopeSkyCafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        private readonly IMapper mapper;
        private readonly ITelegramNotificationService telegramNotificationService;
        private readonly IUserRepository userRepository;

        public OrdersController(IOrderRepository orderRepository, IMapper mapper, ITelegramNotificationService telegramNotificationService, IUserRepository userRepository)
        {
            this.orderRepository = orderRepository;
            this.mapper = mapper;
            this.telegramNotificationService = telegramNotificationService;
            this.userRepository = userRepository;
        }

        // =====================================
        // User Create Order
        // =====================================

        [HttpPost]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> CreateOrder([FromBody] AddOrdersDTO addOrdersDTO)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // ============================================
            // Map DTO -> Order
            // ============================================

            var orderDomain = mapper.Map<Order>(addOrdersDTO);

            // ============================================
            // Get User Id From JWT
            // ============================================

            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identity.");
            }

            orderDomain.UserId = userId;
            orderDomain.Status = OrderStatus.Pending;

            try
            {
                // =====================================================
                // IMPORTANT:
                // Get the result returned from Repository
                // =====================================================

                var orderResult =
                    await orderRepository.CreateOrderAsync(orderDomain);
                var user = await userRepository.GetUserByIdAsync(userId);

                if (user != null)
                {
                    var customerName = $"{user.FirstName} {user.LastName}".Trim();

                    var paymentMethod =
                        orderResult.Order.PaymentMethod == PaymentMethod.Wallet
                            ? "Wallet"
                            : "Cash";

                    var items = orderResult.Order.OrderItems
                        .Select(item => (
                            ProductName: item.Product?.Name ?? string.Empty,
                            Quantity: item.Quantity,
                            UnitPrice: item.Price
                        ))
                        .ToList();

                    await telegramNotificationService.SendNewOrderNotificationAsync(
                        orderResult.Order.Id,
                        orderResult.Order.OrderNumber,
                        customerName,
                        user.PhoneNumber,
                        orderResult.Order.TotalPrice,
                        paymentMethod,
                        orderResult.PaidFromWallet,
                        orderResult.AddedToDebt,
                        items);
                }

                // =====================================================
                // Response
                // =====================================================

                var response = new CreateOrderResponseDTO
                {
                    Message = "Order created successfully",

                    TotalPrice = orderResult.Order.TotalPrice,

                    PaymentMethod = orderResult.Order.PaymentMethod,

                    // ==============================
                    // Before
                    // ==============================

                    WalletBalanceBefore =
                        orderResult.WalletBalanceBefore,

                    DebtBefore =
                        orderResult.DebtBefore,

                    // ==============================
                    // Payment
                    // ==============================

                    PaidFromWallet =
                        orderResult.PaidFromWallet,

                    AddedToDebt =
                        orderResult.AddedToDebt,

                    // ==============================
                    // After
                    // ==============================

                    WalletBalance =
                        orderResult.RemainingBalance,

                    CurrentDebt =
                        orderResult.CurrentDebt,

                    Products = orderResult.Order.OrderItems
                        .Select(item => new CreateOrderItemResponseDTO
                        {
                            ProductName = item.Product?.Name ?? string.Empty,
                            Quantity = item.Quantity,
                            UnitPrice = item.Price,
                            TotalPrice = item.Price * item.Quantity
                        })
                        .ToList()
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }
        // ==========================
        // Admin Accept Order
        // ==========================
        [HttpPut("{id:guid}/accept")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> AcceptOrder(Guid id)
        {
            try
            {
                var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var order = await orderRepository.AcceptOrderAsync(id, adminId);

                if (order == null)
                {
                    return NotFound("Order not found");
                }

                return Ok(new
                {
                    Message = "Order accepted successfully",
                    Status = order.Status
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        // =====================================
        // User Get My Orders
        // =====================================

        [HttpGet("MyOrders")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var orders = await orderRepository.GetOrdersByUserIdAsync(userId);

            var response = new MyOrdersResponseDTO
            {
                Orders = mapper.Map<List<OrdersDTO>>(orders),
                GrandTotal = orders.Sum(o => o.TotalPrice)
            };

            return Ok(response);
        }

        // =====================================
        // Admin & SuperAdmin Get All Orders
        // =====================================

        [HttpGet]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await orderRepository.GetAllOrdersAsync();

            return Ok(mapper.Map<List<OrdersDTO>>(orders));
        }

        // =====================================
        // Admin Ready Order
        // =====================================

        [HttpPut("{id:guid}/ready")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> ReadyOrder(Guid id)
        {
            try
            {
                var order = await orderRepository.ReadyOrderAsync(id);

                if (order == null)
                {
                    return NotFound("Order not found");
                }

                return Ok(new
                {
                    Message = "Order is ready",
                    Status = order.Status
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        // =====================================
        // Delete Order
        // =====================================

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var deletedOrder = await orderRepository.DeleteOrderAsync(id);

            if (deletedOrder == null)
            {
                return NotFound("Order not found");
            }

            return Ok(mapper.Map<OrdersDTO>(deletedOrder));
        }

        // =====================================
        // Admin & SuperAdmin Get Pending Orders
        // =====================================

        [HttpGet("pending")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> GetPendingOrders()
        {
            var orders = await orderRepository.GetOrdersByStatusAsync(OrderStatus.Pending);

            return Ok(mapper.Map<List<OrdersDTO>>(orders));
        }

        // =====================================
        // Admin & SuperAdmin Get Accepted Orders
        // =====================================

        [HttpGet("accepted")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> GetAcceptedOrders()
        {
            var orders = await orderRepository.GetOrdersByStatusAsync(OrderStatus.Accepted);

            return Ok(mapper.Map<List<OrdersDTO>>(orders));
        }

        // =====================================
        // Admin & SuperAdmin Get Ready Orders
        // =====================================

        [HttpGet("ready")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> GetReadyOrders()
        {
            var orders = await orderRepository.GetOrdersByStatusAsync(OrderStatus.Ready);

            return Ok(mapper.Map<List<OrdersDTO>>(orders));
        }
    }
}