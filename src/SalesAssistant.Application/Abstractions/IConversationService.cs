using SalesAssistant.Domain.Entities;

namespace SalesAssistant.Application.Abstractions;

public interface IConversationService
{
    Task<Conversation> GetOrCreateAsync(Guid storeId, string telegramUserId, string telegramUsername, CancellationToken cancellationToken = default);
    Task AddMessageAsync(Guid conversationId, string text, bool isFromUser, CancellationToken cancellationToken = default);
    Task<string?> FindFaqAnswerAsync(Guid storeId, string question, CancellationToken cancellationToken = default);
}
