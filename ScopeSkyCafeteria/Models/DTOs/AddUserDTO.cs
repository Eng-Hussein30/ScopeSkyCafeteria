using System.ComponentModel.DataAnnotations;

namespace ScopeSkyCafeteria.Models.DTOs
{
    public class AddUserDTO
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string? FirstName { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string? LastName { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}