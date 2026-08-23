namespace ScopeSkyCafeteria.Models.Domain
{
    public class Wallet
    {
        public Guid Id { get; set; }

        // صاحب المحفظة
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // الرصيد المتوفر
        public decimal Balance { get; set; } = 0;

        // الدين المستحق على المستخدم
        public decimal Debt { get; set; } = 0;

        // تاريخ إنشاء المحفظة
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // آخر تحديث
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WalletTransaction> Transactions { get; set; }= new List<WalletTransaction>();
    }
}