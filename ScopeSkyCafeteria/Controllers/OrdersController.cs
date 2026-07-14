using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScopeSkyCafeteria.DTOs;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;
using ScopeSkyCafeteria.Repositories;

namespace ScopeSkyCafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        private readonly IMapper mapper;

        public OrdersController(IOrderRepository orderRepository, IMapper mapper)
        {
            this.orderRepository = orderRepository;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] AddOrdersDTO addOrdersDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var orderDomain = mapper.Map<Order>(addOrdersDTO);
            await orderRepository.CreateOrderAsync(orderDomain);
            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = orderDomain.Id },
                mapper.Map<OrdersDTO>(orderDomain)
            );
        }

        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> UpdateOrder([FromRoute] Guid id, [FromBody] UpdateOrdersDTO updateOrdersDTO)
        {
            var orderDomain = mapper.Map<Order>(updateOrdersDTO);
            var updatedOrder = await orderRepository.UpdateOrderAsync(id, orderDomain);
            if (updatedOrder == null)
                return NotFound("Order not found");
            return Ok(mapper.Map<OrdersDTO>(updatedOrder));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await orderRepository.GetAllOrdersAsync();
            return Ok(mapper.Map<List<OrdersDTO>>(orders));
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetOrderById([FromRoute] Guid id)
        {
            var order = await orderRepository.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound("Order not found");
            return Ok(mapper.Map<OrdersDTO>(order));
        }
        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> DeleteOrder([FromRoute] Guid id)
        {
            var deletedOrder = await orderRepository.DeleteOrderAsync(id);
            if (deletedOrder == null)
                return NotFound("Order not found");
            return Ok(mapper.Map<OrdersDTO>(deletedOrder));
        }
    }
}
