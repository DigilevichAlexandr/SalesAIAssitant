using SalesAssistant.Domain.Enums;

namespace SalesAssistant.Domain.Entities;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public SenderType SenderType { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset SentAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public Conversation Conversation { get; set; } = null!;
}
