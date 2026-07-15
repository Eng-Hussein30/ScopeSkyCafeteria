using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Models.DTOs
{
    public class OrdersDTO
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string? UserName { get; set; }

        public Guid? AdminId { get; set; }

        public string? AdminName { get; set; }

        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<OrderItemDTO> OrderItems { get; set; } = new();
    }
}