namespace ScopeSkyCafeteria.Models.DTOs
{

        public class CreateProductDto
        {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public Guid CategoryId { get; set; }


    }
    
}

