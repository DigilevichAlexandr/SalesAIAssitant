using SalesAssistant.Domain.Entities;

namespace SalesAssistant.Application.Abstractions;

public interface ILeadService
{
    Task<(bool Handled, string ReplyText)> TryHandleLeadCaptureAsync(Store store, Conversation conversation, string incomingText, CancellationToken cancellationToken = default);
}
