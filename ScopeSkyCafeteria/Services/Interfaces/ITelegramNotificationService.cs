using ScopeSkyCafeteria.Models.Domain;

namespace ScopeSkyCafeteria.Services.Interfaces;

public interface ITelegramNotificationService
{
    Task SendNewOrderNotificationAsync(Guid orderId,
        int orderNumber,
        string customerName,
        string? customerPhone,
        string? departmentName,
        decimal totalPrice,
        string paymentMethod,
        decimal paidFromWallet,
        decimal addedToDebt,
        IEnumerable<(string ProductName, int Quantity, decimal UnitPrice)> items);

    Task SendOrderStatusNotificationAsync(long telegramChatId,int orderNumber,OrderStatus status);
}