using Microsoft.AspNetCore.Identity;

namespace ScopeSkyCafeteria.Models.Domain
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public long? TelegramChatId { get; set; }
        public string? DepartmentName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Order> CustomerOrders { get; set; } = new List<Order>();

        public ICollection<Order> AssignedOrders { get; set; } = new List<Order>();
        public Wallet? Wallet { get; set; }
    }
}