namespace ScopeSkyCafeteria.Models.DTOs
{
    public class WalletDTO
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string? UserName { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public decimal Balance { get; set; }

        public decimal Debt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}