using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Models.DTOs
{
    public class WalletTransactionDTO
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public decimal Amount { get; set; }

        public WalletTransactionType Type { get; set; }

        public Guid? OrderId { get; set; }

        public Guid? PerformedByUserId { get; set; }

        public string? PerformedByUserName { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}