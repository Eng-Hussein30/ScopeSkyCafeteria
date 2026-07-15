namespace ScopeSkyCafeteria.DTOs
{
    public class AddOrderItemDTO
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }
}