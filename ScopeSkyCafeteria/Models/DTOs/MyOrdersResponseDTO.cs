using ScopeSkyCafeteria.Models.DTOs;

namespace ScopeSkyCafeteria.DTOs
{
    public class MyOrdersResponseDTO
    {
        public List<OrdersDTO> Orders { get; set; } = new();

        public decimal GrandTotal { get; set; }
    }
}