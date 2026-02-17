using SalesAssistant.Domain.Entities;

namespace SalesAssistant.Application.Abstractions;

public interface IOpenAiService
{
    Task<string> GenerateReplyAsync(Store store, Conversation conversation, string userMessage, CancellationToken cancellationToken = default);
}
