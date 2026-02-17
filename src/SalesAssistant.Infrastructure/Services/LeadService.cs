using Microsoft.Extensions.Logging;
using SalesAssistant.Application.Abstractions;
using SalesAssistant.Domain.Entities;
using SalesAssistant.Domain.Enums;

namespace SalesAssistant.Infrastructure.Services;

public class LeadService : ILeadService
{
    private readonly IRepository<Lead> _leadRepository;
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITelegramService _telegramService;
    private readonly ILogger<LeadService> _logger;

    public LeadService(
        IRepository<Lead> leadRepository,
        IRepository<Conversation> conversationRepository,
        IUnitOfWork unitOfWork,
        ITelegramService telegramService,
        ILogger<LeadService> logger)
    {
        _leadRepository = leadRepository;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _telegramService = telegramService;
        _logger = logger;
    }

    public async Task<(bool Handled, string ReplyText)> TryHandleLeadCaptureAsync(Store store, Conversation conversation, string incomingText, CancellationToken cancellationToken = default)
    {
        if (conversation.LeadCaptureStep == LeadCaptureStep.None && IsOrderIntent(incomingText))
        {
            conversation.LeadCaptureStep = LeadCaptureStep.AwaitingName;
            _conversationRepository.Update(conversation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return (true, "Great! Let's place your order. What's your name?");
        }

        if (conversation.LeadCaptureStep == LeadCaptureStep.AwaitingName)
        {
            conversation.PendingLeadName = incomingText.Trim();
            conversation.LeadCaptureStep = LeadCaptureStep.AwaitingPhone;
            _conversationRepository.Update(conversation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return (true, "Thanks! Please share your phone number.");
        }

        if (conversation.LeadCaptureStep == LeadCaptureStep.AwaitingPhone)
        {
            var lead = new Lead
            {
                StoreId = store.Id,
                ConversationId = conversation.Id,
                TelegramUserId = conversation.TelegramUserId,
                CustomerName = conversation.PendingLeadName,
                CustomerPhone = incomingText.Trim()
            };

            await _leadRepository.AddAsync(lead, cancellationToken);
            conversation.LeadCaptureStep = LeadCaptureStep.Completed;
            conversation.PendingLeadName = string.Empty;
            _conversationRepository.Update(conversation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Lead created: {LeadId} for store {StoreId}", lead.Id, store.Id);

            var ownerText = $"🛒 New lead\nStore: {store.Name}\nName: {lead.CustomerName}\nPhone: {lead.CustomerPhone}\nTelegramUser: {lead.TelegramUserId}";
            await _telegramService.SendMessageAsync(store.TelegramBotToken, store.OwnerTelegramChatId, ownerText, cancellationToken);

            return (true, "Perfect, your request has been sent. Our manager will contact you shortly.");
        }

        return (false, string.Empty);
    }

    private static bool IsOrderIntent(string text)
    {
        var value = text.ToLowerInvariant();
        return value.Contains("order") || value.Contains("buy") || value.Contains("purchase") || value.Contains("заказ");
    }
}
