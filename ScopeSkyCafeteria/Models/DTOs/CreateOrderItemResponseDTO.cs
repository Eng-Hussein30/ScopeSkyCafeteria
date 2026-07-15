namespace ScopeSkyCafeteria.DTOs
{
    public class CreateOrderItemResponseDTO
    {
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }
}