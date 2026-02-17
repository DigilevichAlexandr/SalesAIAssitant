using SalesAssistant.Domain.Enums;

namespace SalesAssistant.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StoreId { get; set; }
    public string TelegramUserId { get; set; } = string.Empty;
    public string TelegramUsername { get; set; } = string.Empty;
    public LeadCaptureStep LeadCaptureStep { get; set; } = LeadCaptureStep.None;
    public string PendingLeadName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public Store Store { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
