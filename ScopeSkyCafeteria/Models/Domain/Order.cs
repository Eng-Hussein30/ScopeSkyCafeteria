namespace ScopeSkyCafeteria.Models.Domain
{
    public class Order
    {
        public Guid Id { get; set; }
        public int OrderNumber { get; set; }
        // صاحب الطلب
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // الأدمن الذي استلم الطلب
        public Guid? AdminId { get; set; }
        public User? Admin { get; set; }

        // الأدمن الذي سلّم الطلب
        public Guid? DeliveredByAdminId { get; set; }
        public User? DeliveredByAdmin { get; set; }

        public DateTime? DeliveredAt { get; set; }
        public decimal TotalPrice { get; set; }


        // طريقة الدفع
        public PaymentMethod PaymentMethod { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public string? DepartmentName { get; set; }

  

    }
}