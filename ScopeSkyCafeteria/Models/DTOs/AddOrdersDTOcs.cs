using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;

namespace ScopeSkyCafeteria.DTOs
{
    public class AddOrdersDTO
    {


        public List<AddOrderItemDTO> OrderItems { get; set; } = new();
    }
}