namespace SalesAssistant.Domain.Enums;

public enum LeadCaptureStep
{
    None = 0,
    AwaitingName = 1,
    AwaitingPhone = 2,
    Completed = 3
}
