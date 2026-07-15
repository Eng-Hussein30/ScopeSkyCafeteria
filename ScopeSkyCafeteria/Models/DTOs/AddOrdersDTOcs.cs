using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;

namespace ScopeSkyCafeteria.DTOs
{
    public class AddOrdersDTO
    {
        public decimal TotalPrice { get; set; }

        public List<AddOrderItemDTO> OrderItems { get; set; } = new();
    }
}