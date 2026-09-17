namespace ScopeSkyCafeteria.Models.DTOs
{
    public class LoginResponseDTO
    {
        public string JwtToken { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}