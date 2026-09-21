using System.Net.Http.Json;
using ScopeSkyCafeteria.Services.Interfaces;

namespace ScopeSkyCafeteria.Services.Implementations;

public class TelegramNotificationService : ITelegramNotificationService
{
    private readonly HttpClient httpClient;
    private readonly IConfiguration configuration;
    private readonly ILogger<TelegramNotificationService> logger;

    public TelegramNotificationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TelegramNotificationService> logger)
    {
        this.httpClient = httpClient;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task SendNewOrderNotificationAsync(
        Guid orderId,
        int orderNumber,
        string customerName,
        string? customerPhone,
        decimal totalPrice,
        string paymentMethod,
        decimal paidFromWallet,
        decimal addedToDebt,
        IEnumerable<(string ProductName, int Quantity, decimal UnitPrice)> items)
    {
        try
        {
            var token = configuration["TELEGRAM_BOT_TOKEN"];
            var chatId = configuration["TELEGRAM_ADMIN_CHAT_ID"];

            if (string.IsNullOrWhiteSpace(token))
            {
                logger.LogWarning(
                    "Telegram notification skipped because TELEGRAM_BOT_TOKEN is missing.");

                return;
            }

            if (string.IsNullOrWhiteSpace(chatId))
            {
                logger.LogWarning(
                    "Telegram notification skipped because TELEGRAM_ADMIN_CHAT_ID is missing.");

                return;
            }

            // رابط الواجهة الأمامية
            var frontendBaseUrl = configuration["FRONTEND_BASE_URL"];

            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                logger.LogWarning(
                    "Telegram notification skipped because FRONTEND_BASE_URL is missing.");

                return;
            }

            frontendBaseUrl = frontendBaseUrl.TrimEnd('/');

            // رابط صفحة الطلب في الـ Frontend
            // الـ GUID موجود داخل الرابط فقط ولن يظهر في رسالة Telegram
            var orderUrl = $"{frontendBaseUrl}/admin/orders/{orderId}";

            var message = BuildOrderMessage(
                orderNumber,
                customerName,
                customerPhone,
                totalPrice,
                paymentMethod,
                paidFromWallet,
                addedToDebt,
                items);

            var url =
                $"https://api.telegram.org/bot{token}/sendMessage";

            var request = new
            {
                chat_id = chatId,
                text = message,

                // زر فتح الطلب
                reply_markup = new
                {
                    inline_keyboard = new[]
                    {
                        new[]
                        {
                            new
                            {
                                text = "🔗 فتح الطلب",
                                url = orderUrl
                            }
                        }
                    }
                }
            };

            var response =
                await httpClient.PostAsJsonAsync(url, request);

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                logger.LogWarning(
                    "Telegram notification failed. StatusCode: {StatusCode}, Response: {Response}",
                    response.StatusCode,
                    error);

                return;
            }

            logger.LogInformation(
                "Telegram notification sent successfully for order {OrderId}.",
                orderId);
        }
        catch (Exception ex)
        {
            // Telegram failure must NOT affect the order.
            logger.LogError(
                ex,
                "Failed to send Telegram notification for order {OrderId}.",
                orderId);
        }
    }

    private static string BuildOrderMessage(
        int orderNumber,
        string customerName,
        string? customerPhone,
        decimal totalPrice,
        string paymentMethod,
        decimal paidFromWallet,
        decimal addedToDebt,
        IEnumerable<(string ProductName, int Quantity, decimal UnitPrice)> items)
    {
        var message = $"""
            🔔 طلب جديد

            👤 الزبون: {customerName}
            📱 الهاتف: {customerPhone ?? "غير متوفر"}

            🛒 الطلب:
            """;

        foreach (var item in items)
        {
            var itemTotal = item.UnitPrice * item.Quantity;

            message +=
                $"\n• {item.ProductName} × {item.Quantity} — {itemTotal:N0} IQD";
        }

        message += $"""

            
            💰 المبلغ الكلي: {totalPrice:N0} IQD
            💳 طريقة الدفع: {paymentMethod}
            """;

        if (paymentMethod == "Wallet")
        {
            message += $"""

                
                💵 المدفوع من المحفظة: {paidFromWallet:N0} IQD
                📒 المضاف للدين: {addedToDebt:N0} IQD
                """;
        }

        message += $"""

            
            📦 الحالة: Pending
            🔢 رقم الطلب: {orderNumber}
            """;

        return message;
    }
}