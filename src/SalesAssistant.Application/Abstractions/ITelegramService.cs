using SalesAssistant.Application.Models;

namespace SalesAssistant.Application.Abstractions;

public interface ITelegramService
{
    Task SendMessageAsync(string botToken, string chatId, string text, CancellationToken cancellationToken = default);
    Task ProcessIncomingUpdateAsync(Guid storeId, TelegramUpdate update, CancellationToken cancellationToken = default);
}
