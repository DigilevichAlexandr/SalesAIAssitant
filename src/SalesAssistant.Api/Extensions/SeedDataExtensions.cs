using Microsoft.EntityFrameworkCore;
using SalesAssistant.Domain.Entities;
using SalesAssistant.Infrastructure.Persistence;

namespace SalesAssistant.Api.Extensions;

public static class SeedDataExtensions
{
    public static async Task SeedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();

        if (await db.Stores.AnyAsync())
        {
            return;
        }

        var store = new Store
        {
            Name = "Demo Telegram Store",
            TelegramBotToken = "SET_TELEGRAM_BOT_TOKEN",
            OwnerTelegramChatId = "SET_OWNER_CHAT_ID",
            SystemPrompt = "You are a sales assistant for an e-commerce Telegram shop. Answer politely, push toward order completion, and collect missing details."
        };

        db.Stores.Add(store);
        db.Faqs.AddRange(
            new Faq { Store = store, Question = "delivery", Answer = "Delivery takes 1-3 business days depending on your location." },
            new Faq { Store = store, Question = "payment", Answer = "We support card transfer and cash on delivery." },
            new Faq { Store = store, Question = "return", Answer = "You can return unused items within 14 days." });

        await db.SaveChangesAsync();
    }
}
