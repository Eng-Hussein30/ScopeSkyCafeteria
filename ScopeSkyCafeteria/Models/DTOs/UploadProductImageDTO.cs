using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ScopeSkyCafeteria.DTOs
{
    public class UploadProductImageDTO
    {
        [Required]
        public IFormFile Image { get; set; } = null!;
    }
}