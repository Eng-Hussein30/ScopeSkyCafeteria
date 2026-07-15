using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScopeSkyCafeteria.DTOs;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;
using ScopeSkyCafeteria.Repositories;
using System.Security.Claims;

namespace ScopeSkyCafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        private readonly IMapper mapper;

        public OrdersController(IOrderRepository orderRepository, IMapper mapper)
        {
            this.orderRepository = orderRepository;
            this.mapper = mapper;
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

            var orderDomain = mapper.Map<Order>(addOrdersDTO);

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            orderDomain.UserId = userId;
            orderDomain.Status = OrderStatus.Pending;

            await orderRepository.CreateOrderAsync(orderDomain);

            var response = new CreateOrderResponseDTO
            {
                Message = "Order created successfully",
                TotalPrice = orderDomain.TotalPrice,
                Products = orderDomain.OrderItems.Select(item => new CreateOrderItemResponseDTO
                {
                    ProductName = item.Product?.Name ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    TotalPrice = item.Price * item.Quantity
                }).ToList()
            };

            return Ok(response);
        }

        // =====================================
        // User Get My Orders
        // =====================================

        [HttpGet("MyOrders")]
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
        // Get Order By Id
        // =====================================

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await orderRepository.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(mapper.Map<OrdersDTO>(order));
        }

        // =====================================
        // Admin Accept Order
        // =====================================

        [HttpPut("{id:guid}/Accept")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> AcceptOrder(Guid id)
        {
            var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var order = await orderRepository.AcceptOrderAsync(id, adminId);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(mapper.Map<OrdersDTO>(order));
        }

        // =====================================
        // Change Order Status
        // =====================================

        [HttpPut("{id:guid}/Status")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> ChangeStatus(Guid id, OrderStatus status)
        {
            var order = await orderRepository.ChangeStatusAsync(id, status);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(mapper.Map<OrdersDTO>(order));
        }

        // =====================================
        // Update Order
        // =====================================

        [HttpPut("{id:guid}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> UpdateOrder(
            Guid id,
            [FromBody] UpdateOrdersDTO updateOrdersDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderDomain = mapper.Map<Order>(updateOrdersDTO);

            var updatedOrder = await orderRepository.UpdateOrderAsync(id, orderDomain);

            if (updatedOrder == null)
            {
                return NotFound("Order not found");
            }

            return Ok(mapper.Map<OrdersDTO>(updatedOrder));
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
    }
}