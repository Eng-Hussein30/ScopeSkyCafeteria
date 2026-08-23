namespace ScopeSkyCafeteria.Models.Domain
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }

        // المحفظة المرتبطة بالعملية
        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; } = null!;

        // قيمة العملية
        public decimal Amount { get; set; }

        // نوع العملية
        public WalletTransactionType Type { get; set; }

        // الطلب المرتبط بالعملية - اختياري
        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }

        // المستخدم الذي قام بالعملية
        // Admin / SuperAdmin عند التعديل
        public Guid? PerformedByUserId { get; set; }
        public User? PerformedByUser { get; set; }

        // ملاحظات اختيارية
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}