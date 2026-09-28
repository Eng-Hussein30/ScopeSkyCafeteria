using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Services.Interfaces;
using System.Net.Http.Json;

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
        string? departmentName,
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

            var frontendBaseUrl = configuration["FRONTEND_BASE_URL"];

            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                logger.LogWarning(
                    "Telegram notification skipped because FRONTEND_BASE_URL is missing.");

                return;
            }

            frontendBaseUrl = frontendBaseUrl.TrimEnd('/');

            var orderUrl = $"{frontendBaseUrl}/admin/orders/{orderId}";

            var message = BuildOrderMessage(
                orderNumber,
                customerName,
                customerPhone,
                departmentName,
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
            logger.LogError(
                ex,
                "Failed to send Telegram notification for order {OrderId}.",
                orderId);
        }
    }

    public async Task SendOrderStatusNotificationAsync(long telegramChatId,int orderNumber,OrderStatus status)
    
    {
        try
        {
            var token = configuration["TELEGRAM_BOT_TOKEN"];

            if (string.IsNullOrWhiteSpace(token))
            {
                logger.LogWarning("Telegram status notification skipped because TELEGRAM_BOT_TOKEN is missing.");

                return;
            }

            var statusMessage = status switch
            {
                OrderStatus.Pending =>"⏳ تم إنشاء طلبك بنجاح.",

                OrderStatus.Accepted =>"✅ تم قبول طلبك من قبل الإدارة.",

                OrderStatus.Ready =>"📦 طلبك أصبح جاهزًا.",

                OrderStatus.OnTheWay =>"🚚 طلبك في الطريق إليك.",

                OrderStatus.Delivered =>"🎉 تم تسليم طلبك بنجاح.",
                
                _ => null
            };

            if (statusMessage == null)
                return;

            var message = $"""
                             🔔 تحديث الطلب
 
                             🔢 رقم الطلب: {orderNumber}

                                           {statusMessage}
                           """;

            var url =$"https://api.telegram.org/bot{token}/sendMessage";

            var request = new
            {
                chat_id = telegramChatId,
                text = message
            };

            var response =await httpClient.PostAsJsonAsync(url, request);

            if (!response.IsSuccessStatusCode)
            {
                var error =await response.Content.ReadAsStringAsync();

                logger.LogWarning("Telegram status notification failed. StatusCode: {StatusCode}, Response: {Response}",response.StatusCode,error);

                return;
            }

            logger.LogInformation("Telegram status notification sent for order {OrderNumber}.",orderNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,"Failed to send Telegram status notification for order {OrderNumber}.",orderNumber);
        }
    }

    public async Task SendOrderDeliveredToAdminNotificationAsync(
    int orderNumber,
    string customerName,
    string? departmentName)
    {
        try
        {
            var token = configuration["TELEGRAM_BOT_TOKEN"];
            var chatId = configuration["TELEGRAM_ADMIN_CHAT_ID"];

            if (string.IsNullOrWhiteSpace(token))
            {
                logger.LogWarning(
                    "Telegram delivery notification skipped because TELEGRAM_BOT_TOKEN is missing.");

                return;
            }

            if (string.IsNullOrWhiteSpace(chatId))
            {
                logger.LogWarning(
                    "Telegram delivery notification skipped because TELEGRAM_ADMIN_CHAT_ID is missing.");

                return;
            }

            var message = $"""
                       🎉 تم تسليم الطلب بنجاح

                       🔢 رقم الطلب: {orderNumber}
                       👤 المستخدم: {customerName}
                       🏢 القسم: {departmentName ?? "غير محدد"}

                       تم تسليم الطلب للمستخدم بهذا القسم بنجاح.
                       """;

            var url =
                $"https://api.telegram.org/bot{token}/sendMessage";

            var request = new
            {
                chat_id = chatId,
                text = message
            };

            var response =
                await httpClient.PostAsJsonAsync(url, request);

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                logger.LogWarning(
                    "Telegram admin delivery notification failed. StatusCode: {StatusCode}, Response: {Response}",
                    response.StatusCode,
                    error);

                return;
            }

            logger.LogInformation(
                "Telegram admin delivery notification sent for order {OrderNumber}.",
                orderNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send Telegram admin delivery notification for order {OrderNumber}.",
                orderNumber);
        }
    }

    private static string BuildOrderMessage(
        int orderNumber,
        string customerName,
        string? customerPhone,
        string? departmentName,
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
                           📍 القسم: {departmentName ?? "غير محدد"}

                               🛒 الطلب:
                     """;

        foreach (var item in items)
        {
            var itemTotal = item.UnitPrice * item.Quantity;

            message +=$"\n• {item.ProductName} × {item.Quantity} — {itemTotal:N0} IQD";
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
