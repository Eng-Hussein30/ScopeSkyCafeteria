using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.DTOs
{
    public class CreateOrderResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        // مجموع الطلب
        public decimal TotalPrice { get; set; }

        // طريقة الدفع
        public PaymentMethod PaymentMethod { get; set; }

        // ==============================
        // Before
        // ==============================

        // رصيد المحفظة قبل الخصم
        public decimal WalletBalanceBefore { get; set; }

        // الدين قبل الطلب
        public decimal DebtBefore { get; set; }

        // ==============================
        // Payment
        // ==============================

        // المبلغ المدفوع من المحفظة
        public decimal PaidFromWallet { get; set; }

        // المبلغ المضاف إلى الدين
        public decimal AddedToDebt { get; set; }

        // ==============================
        // After
        // ==============================

        // رصيد المحفظة بعد الخصم
        public decimal WalletBalance { get; set; }

        // الدين بعد العملية
        public decimal CurrentDebt { get; set; }

        // المنتجات
        public List<CreateOrderItemResponseDTO> Products { get; set; } = new();
    }
}