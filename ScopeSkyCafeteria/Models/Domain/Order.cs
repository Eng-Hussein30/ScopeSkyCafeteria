namespace ScopeSkyCafeteria.Models.Domain
{
    public class Order
    {
        public Guid Id { get; set; }

        // صاحب الطلب
        public Guid UserId { get; set; }
        public User User { get; set; }

        // الأدمن الذي استلم الطلب
        public Guid? AdminId { get; set; }
        public User? Admin { get; set; }

        public decimal TotalPrice { get; set; }

        // طريقة الدفع
        public PaymentMethod PaymentMethod { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}