namespace SalesAssistant.Application.Models;

public class TelegramUpdate
{
    public TelegramMessage? Message { get; set; }
}

public class TelegramMessage
{
    public long MessageId { get; set; }
    public TelegramChat Chat { get; set; } = new();
    public TelegramUser? From { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class TelegramChat
{
    public long Id { get; set; }
}

public class TelegramUser
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
}
