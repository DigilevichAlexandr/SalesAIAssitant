namespace SalesAssistant.Domain.Entities;

public class Faq
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StoreId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;

    public Store Store { get; set; } = null!;
}
