using System.ComponentModel.DataAnnotations;

namespace ScopeSkyCafeteria.Models.DTOs
{
    public class LoginRequestDTO
    {

        [Required]

        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
