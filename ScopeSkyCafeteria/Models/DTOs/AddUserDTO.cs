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
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
