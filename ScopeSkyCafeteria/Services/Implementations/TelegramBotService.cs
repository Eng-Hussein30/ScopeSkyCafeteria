using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScopeSkyCafeteria.Models.Domain;
using System.Text.Json.Serialization;

namespace ScopeSkyCafeteria.Services.Implementations;

public class TelegramBotService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IConfiguration configuration;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<TelegramBotService> logger;

    private long? lastUpdateId;

    public TelegramBotService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<TelegramBotService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.configuration = configuration;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var token =
            configuration["TELEGRAM_BOT_TOKEN"];

        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogWarning(
                "Telegram bot disabled because TELEGRAM_BOT_TOKEN is missing.");

            return;
        }

        logger.LogInformation(
            "Telegram bot service started.");

        var httpClient =
            httpClientFactory.CreateClient();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates =
                    await GetUpdatesAsync(
                        httpClient,
                        token,
                        stoppingToken);

                foreach (var update in updates)
                {
                    if (update.UpdateId > (lastUpdateId ?? 0))
                    {
                        lastUpdateId = update.UpdateId;

                        await HandleUpdateAsync(
                            httpClient,
                            token,
                            update,
                            stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Telegram bot polling failed.");

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }

        logger.LogInformation(
            "Telegram bot service stopped.");
    }

    // =====================================================
    // Get Updates
    // =====================================================

    private async Task<List<TelegramUpdate>> GetUpdatesAsync(
        HttpClient httpClient,
        string token,
        CancellationToken cancellationToken)
    {
        var url =
            $"https://api.telegram.org/bot{token}/getUpdates";

        var request = new
        {
            offset = (lastUpdateId ?? 0) + 1,
            timeout = 30
        };

        using var response =
            await httpClient.PostAsJsonAsync(
                url,
                request,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Telegram getUpdates failed: {error}");
        }

        var result =
            await response.Content
                .ReadFromJsonAsync<TelegramResponse<List<TelegramUpdate>>>(
                    cancellationToken);

        return result?.Result ?? new List<TelegramUpdate>();
    }

    // =====================================================
    // Handle Update
    // =====================================================

    private async Task HandleUpdateAsync(
        HttpClient httpClient,
        string token,
        TelegramUpdate update,
        CancellationToken cancellationToken)
    {
        if (update.Message == null)
        {
            return;
        }

        var message =
            update.Message;

        var chatId =
            message.Chat.Id;

        // =================================================
        // Contact Shared
        // =================================================

        if (message.Contact != null)
        {
            await HandleContactAsync(
                httpClient,
                token,
                message,
                cancellationToken);

            return;
        }

        var text =
            message.Text?.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        // =================================================
        // /start
        // =================================================

        if (text.Equals(
                "/start",
                StringComparison.OrdinalIgnoreCase))
        {
            await SendPhoneRequestAsync(
                httpClient,
                token,
                chatId,
                cancellationToken);

            return;
        }

        // =================================================
        // My Orders
        // =================================================

        if (text == "📦 طلباتي")
        {
            await SendMyOrdersAsync(
                httpClient,
                token,
                chatId,
                cancellationToken);

            return;
        }

        // =================================================
        // Unknown Command
        // =================================================

        await SendTextAsync(
            httpClient,
            token,
            chatId,
            "استخدم /start لربط حسابك، أو اختر 📦 طلباتي.",
            cancellationToken);
    }

    // =====================================================
    // Handle Contact
    // =====================================================

    private async Task HandleContactAsync(
        HttpClient httpClient,
        string token,
        TelegramMessage message,
        CancellationToken cancellationToken)
    {
        var contact =
            message.Contact!;

        var chatId =
            message.Chat.Id;

        // يجب أن يكون الرقم المشارك هو رقم صاحب حساب Telegram
        if (contact.UserId.HasValue &&
            contact.UserId.Value != message.From?.Id)
        {
            await SendTextAsync(
                httpClient,
                token,
                chatId,
                "⚠️ يجب مشاركة رقم هاتفك أنت، وليس رقم شخص آخر.",
                cancellationToken);

            return;
        }

        var phoneNumber =
            NormalizePhoneNumber(contact.PhoneNumber);

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            await SendTextAsync(
                httpClient,
                token,
                chatId,
                "❌ لم أتمكن من قراءة رقم الهاتف.",
                cancellationToken);

            return;
        }

        using var scope =
            scopeFactory.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<User>>();

        var user =
            await userManager.Users
                .FirstOrDefaultAsync(
                    u => u.PhoneNumber == phoneNumber,
                    cancellationToken);

        if (user == null)
        {
            await SendTextAsync(
                httpClient,
                token,
                chatId,
                """
                ❌ لم يتم العثور على حساب بهذا الرقم.

                تأكد أن رقم الهاتف في حساب الكافتريا مطابق للرقم الذي شاركته هنا.
                """,
                cancellationToken);

            return;
        }

        user.TelegramChatId =
            chatId;

        await userManager.UpdateAsync(user);

        await SendMainMenuAsync(
            httpClient,
            token,
            chatId,
            $"✅ تم ربط حسابك بنجاح، {user.FirstName} {user.LastName}.\n\nيمكنك الآن متابعة طلباتك من خلال 📦 طلباتي.",
            cancellationToken);
    }

    // =====================================================
    // Send Phone Request
    // =====================================================

    private async Task SendPhoneRequestAsync(
        HttpClient httpClient,
        string token,
        long chatId,
        CancellationToken cancellationToken)
    {
        var url =
            $"https://api.telegram.org/bot{token}/sendMessage";

        var request = new
        {
            chat_id = chatId,

            text ="""
                     👋 أهلاً بك في بوت ScopeSky Cafeteria.

                            لربط حسابك، اضغط على الزر أدناه وشارك رقم الهاتف المسجل في حساب الكافتريا.
                   """,

            reply_markup = new
            {
                keyboard = new[]
                {
                    new[]
                    {
                        new
                        {
                            text = "📱 مشاركة رقم الهاتف",
                            request_contact = true
                        }
                    }
                },

                resize_keyboard = true,

                one_time_keyboard = true
            }
        };

        await httpClient.PostAsJsonAsync(
            url,
            request,
            cancellationToken);
    }

    // =====================================================
    // Send Main Menu
    // =====================================================

    private async Task SendMainMenuAsync(
                                         HttpClient httpClient,
                                         string token,
                                         long chatId,
                                         string message,
                                         CancellationToken cancellationToken)
    {
        var url =$"https://api.telegram.org/bot{token}/sendMessage";

        var request = new
        {
            chat_id = chatId,

            text = message,

            reply_markup = new
            {
                keyboard = new[]
                {
                    new[]
                    {
                        new
                        {
                            text = "📦 طلباتي"
                        }
                    }
                },

                resize_keyboard = true
            }
        };

        await httpClient.PostAsJsonAsync(url,request,cancellationToken);
    }

    // =====================================================
    // Send My Orders
    // =====================================================

    private async Task SendMyOrdersAsync(
                                         HttpClient httpClient,
                                         string token,
                                         long chatId,
                                         CancellationToken cancellationToken)
    {
        using var scope =scopeFactory.CreateScope();

        var userManager =scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var orderRepository =scope.ServiceProvider.GetRequiredService<ScopeSkyCafeteria.Repositories.IOrderRepository>();

        var user =await userManager.Users.FirstOrDefaultAsync(u => u.TelegramChatId == chatId,cancellationToken);

        if (user == null)
        {
            await SendTextAsync(httpClient,token,chatId,
                                                           """
                                                                ⚠️ حسابك غير مربوط.

                                                                   استخدم /start لربط حسابك برقم الهاتف.
                                                            """,
                                cancellationToken);

            return;
        }

        var orders =
            await orderRepository.GetOrdersByUserIdAsync(
                user.Id);

        if (orders.Count == 0)
        {
            await SendTextAsync(
                httpClient,
                token,
                chatId,
                "📦 لا توجد لديك طلبات حاليًا.",
                cancellationToken);

            return;
        }

        var message =
            "📦 طلباتك\n\n";

        foreach (var order in orders
                     .OrderByDescending(o => o.CreatedAt)
                     .Take(20))
        {
            var status =
                GetStatusText(order.Status);

            message +=
                $"🔢 الطلب #{order.OrderNumber}\n" +
                $"📅 {order.CreatedAt:yyyy-MM-dd HH:mm}\n" +
                $"💰 {order.TotalPrice:N0} IQD\n" +
                $"📌 الحالة: {status}\n\n";
        }

        await SendTextAsync(
            httpClient,
            token,
            chatId,
            message,
            cancellationToken);
    }

    // =====================================================
    // Status Text
    // =====================================================

    private static string GetStatusText(
        OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending =>"⏳ قيد الانتظار",

            OrderStatus.Accepted =>"✅ تم القبول",

            OrderStatus.Preparing =>"👨‍🍳 قيد التحضير",

            OrderStatus.Ready =>"📦 جاهز",

            OrderStatus.OnTheWay =>"🚚 في الطريق",

            OrderStatus.Delivered =>"🎉 تم التسليم",

            OrderStatus.Cancelled =>"❌ ملغى",

            _ =>
                status.ToString()
        };
    }

    // =====================================================
    // Send Text
    // =====================================================

    private async Task SendTextAsync(HttpClient httpClient,string token,long chatId,string text,CancellationToken cancellationToken)
    {
        var url =$"https://api.telegram.org/bot{token}/sendMessage";

        var request = new
        {
            chat_id = chatId,
            text
        };

        await httpClient.PostAsJsonAsync(url,request,cancellationToken);
    }

    // =====================================================
    // Normalize Phone Number
    // =====================================================

    private static string NormalizePhoneNumber(
        string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
        }

        var digits =
            new string(phoneNumber.Where(char.IsDigit).ToArray());

                    // Telegram قد يرسل رقم العراق بالشكل:
                    // 9647XXXXXXXXX
                    //
                    // وإذا كانت قاعدة البيانات تحتوي:
                    // 07XXXXXXXXX
                    //
                    // نوحد الاثنين إلى:
                    // 7XXXXXXXXX

        if (digits.StartsWith("964"))
        {
            digits =
                digits[3..];

            if (!digits.StartsWith("0"))
            {
                digits = "0" + digits;
            }
        }

        return digits;
    }
}

// =========================================================
// Telegram DTOs
// =========================================================

public class TelegramResponse<T>
{
    public bool Ok { get; set; }

    public T? Result { get; set; }
}

         public class TelegramUpdate
         {
               [JsonPropertyName("update_id")]
                public long UpdateId { get; set; }

                [JsonPropertyName("message")]
                public TelegramMessage? Message { get; set; }
         }

public class TelegramMessage
{
    [JsonPropertyName("message_id")]
    public long MessageId { get; set; }

    [JsonPropertyName("chat")]
    public TelegramChat Chat { get; set; } = new();

    [JsonPropertyName("from")]
    public TelegramUser? From { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("contact")]
    public TelegramContact? Contact { get; set; }
}

public class TelegramChat
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
}

public class TelegramUser
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
}

public class TelegramContact
{
    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }
}