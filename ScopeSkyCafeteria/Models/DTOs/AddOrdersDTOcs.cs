namespace ScopeSkyCafeteria.Models.DTOs
{
    public class AddOrdersDTO
    {
        public Guid UserId { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = "Pending";
    }
}