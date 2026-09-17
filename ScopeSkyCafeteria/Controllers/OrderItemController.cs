using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScopeSkyCafeteria.DTOs;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;
using ScopeSkyCafeteria.Repositories;

namespace ScopeSkyCafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IOrderItemRepository orderItemRepository;

        public OrderItemController(
            IMapper mapper,
            IOrderItemRepository orderItemRepository)
        {
            this.mapper = mapper;
            this.orderItemRepository = orderItemRepository;
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] AddOrderItemDTO addOrderItemDTO)
        {
            var orderItem = mapper.Map<OrderItem>(addOrderItemDTO);

            await orderItemRepository.CreateOrderItemAsync(orderItem);

            return Ok(mapper.Map<OrderItemDTO>(orderItem));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orderItems =
                await orderItemRepository.GetAllOrderItemsAsync();

            return Ok(mapper.Map<List<OrderItemDTO>>(orderItems));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var orderItem =
                await orderItemRepository.GetOrderItemByIdAsync(id);

            if (orderItem == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<OrderItemDTO>(orderItem));
        }
    }
}