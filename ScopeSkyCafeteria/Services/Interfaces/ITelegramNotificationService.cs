namespace ScopeSkyCafeteria.Services.Interfaces;

public interface ITelegramNotificationService
{
    Task SendNewOrderNotificationAsync(
        Guid orderId,
        string customerName,
        string? customerPhone,
        decimal totalPrice,
        string paymentMethod,
        decimal paidFromWallet,
        decimal addedToDebt,
        IEnumerable<(string ProductName, int Quantity, decimal UnitPrice)> items);
}