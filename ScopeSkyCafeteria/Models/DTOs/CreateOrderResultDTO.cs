using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.DTOs
{
    public class CreateOrderResultDTO
    {
        public Order Order { get; set; } = null!;

        // المحفظة قبل الخصم
        public decimal WalletBalanceBefore { get; set; }

        // الدين قبل الطلب
        public decimal DebtBefore { get; set; }

        // المبلغ المدفوع من المحفظة
        public decimal PaidFromWallet { get; set; }

        // المبلغ الذي أضيف إلى الدين
        public decimal AddedToDebt { get; set; }

        // المحفظة بعد الخصم
        public decimal RemainingBalance { get; set; }

        // الدين بعد العملية
        public decimal CurrentDebt { get; set; }
    }
}