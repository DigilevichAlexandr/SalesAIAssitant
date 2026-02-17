namespace SalesAssistant.Domain.Entities;

public class Store
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string TelegramBotToken { get; set; } = string.Empty;
    public string OwnerTelegramChatId { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = "You are a helpful sales assistant for Telegram store customers. Keep replies concise and conversion-focused.";

    public ICollection<Faq> Faqs { get; set; } = new List<Faq>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}
