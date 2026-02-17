using Microsoft.Extensions.Logging;
using SalesAssistant.Application.Abstractions;
using SalesAssistant.Domain.Entities;

namespace SalesAssistant.Infrastructure.Services;

public class ConversationService : IConversationService
{
    private readonly IRepository<Conversation> _conversationRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<Faq> _faqRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(
        IRepository<Conversation> conversationRepository,
        IRepository<Message> messageRepository,
        IRepository<Faq> faqRepository,
        IUnitOfWork unitOfWork,
        ILogger<ConversationService> logger)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _faqRepository = faqRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Conversation> GetOrCreateAsync(Guid storeId, string telegramUserId, string telegramUsername, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.FirstOrDefaultAsync(
            x => x.StoreId == storeId && x.TelegramUserId == telegramUserId,
            cancellationToken);

        if (conversation is not null)
        {
            conversation.UpdatedAtUtc = DateTimeOffset.UtcNow;
            conversation.TelegramUsername = telegramUsername;
            _conversationRepository.Update(conversation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return conversation;
        }

        conversation = new Conversation
        {
            StoreId = storeId,
            TelegramUserId = telegramUserId,
            TelegramUsername = telegramUsername
        };

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Created conversation {ConversationId} for telegram user {TelegramUserId}", conversation.Id, telegramUserId);

        return conversation;
    }

    public async Task AddMessageAsync(Guid conversationId, string text, bool isFromUser, CancellationToken cancellationToken = default)
    {
        var message = new Message
        {
            ConversationId = conversationId,
            SenderType = isFromUser ? Domain.Enums.SenderType.User : Domain.Enums.SenderType.Assistant,
            Text = text
        };

        await _messageRepository.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> FindFaqAnswerAsync(Guid storeId, string question, CancellationToken cancellationToken = default)
    {
        var normalizedQuestion = question.Trim().ToLowerInvariant();
        var faqs = await _faqRepository.ListAsync(x => x.StoreId == storeId, cancellationToken);
        var faq = faqs.FirstOrDefault(x => normalizedQuestion.Contains(x.Question.Trim().ToLowerInvariant()));
        return faq?.Answer;
    }
}
