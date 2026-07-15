namespace ScopeSkyCafeteria.DTOs
{
    public class CreateOrderResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }

        public List<CreateOrderItemResponseDTO> Products { get; set; } = new();
    }
}