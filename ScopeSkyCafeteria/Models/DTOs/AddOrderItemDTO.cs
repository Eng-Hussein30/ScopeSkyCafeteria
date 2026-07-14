namespace ScopeSkyCafeteria.Models.DTOs
{
    public class AddOrderItemDTO
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
