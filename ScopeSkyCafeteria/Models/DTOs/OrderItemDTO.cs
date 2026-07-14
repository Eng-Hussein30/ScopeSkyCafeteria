using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Models.DTOs
{
    public class OrderItemDTO
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        public decimal Price { get; set; }
    }
}
