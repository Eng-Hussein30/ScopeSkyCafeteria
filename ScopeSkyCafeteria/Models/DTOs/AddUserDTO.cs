using System.ComponentModel.DataAnnotations;

public class AddUserDTO
{
    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string LastName { get; set; } = string.Empty;

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

    public string? DepartmentName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}