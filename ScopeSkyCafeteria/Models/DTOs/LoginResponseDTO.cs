namespace ScopeSkyCafeteria.Models.DTOs
{
    public class LoginResponseDTO
    {
        public string JwtToken { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }
}
