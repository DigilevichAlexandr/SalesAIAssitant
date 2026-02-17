using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SalesAssistant.Application.Abstractions;
using SalesAssistant.Application.Models;
using SalesAssistant.Domain.Entities;

namespace SalesAssistant.Infrastructure.Services;

public class TelegramService : ITelegramService
{
    private readonly IRepository<Store> _storeRepository;
    private readonly IConversationService _conversationService;
    private readonly ILeadService _leadService;
    private readonly IOpenAiService _openAiService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(
        IRepository<Store> storeRepository,
        IConversationService conversationService,
        ILeadService leadService,
        IOpenAiService openAiService,
        IHttpClientFactory httpClientFactory,
        ILogger<TelegramService> logger)
    {
        _storeRepository = storeRepository;
        _conversationService = conversationService;
        _leadService = leadService;
        _openAiService = openAiService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task ProcessIncomingUpdateAsync(Guid storeId, TelegramUpdate update, CancellationToken cancellationToken = default)
    {
        var telegramMessage = update.Message;
        if (telegramMessage is null || string.IsNullOrWhiteSpace(telegramMessage.Text) || telegramMessage.From is null)
        {
            return;
        }

        var store = await _storeRepository.FirstOrDefaultAsync(x => x.Id == storeId, cancellationToken);
        if (store is null)
        {
            _logger.LogWarning("Store {StoreId} not found for incoming telegram update", storeId);
            return;
        }

        var conversation = await _conversationService.GetOrCreateAsync(
            store.Id,
            telegramMessage.From.Id.ToString(),
            telegramMessage.From.Username,
            cancellationToken);

        await _conversationService.AddMessageAsync(conversation.Id, telegramMessage.Text, true, cancellationToken);

        var leadResult = await _leadService.TryHandleLeadCaptureAsync(store, conversation, telegramMessage.Text, cancellationToken);
        if (leadResult.Handled)
        {
            await _conversationService.AddMessageAsync(conversation.Id, leadResult.ReplyText, false, cancellationToken);
            await SendMessageAsync(store.TelegramBotToken, telegramMessage.Chat.Id.ToString(), leadResult.ReplyText, cancellationToken);
            return;
        }

        var faqAnswer = await _conversationService.FindFaqAnswerAsync(store.Id, telegramMessage.Text, cancellationToken);
        var answer = faqAnswer ?? await _openAiService.GenerateReplyAsync(store, conversation, telegramMessage.Text, cancellationToken);

        await _conversationService.AddMessageAsync(conversation.Id, answer, false, cancellationToken);
        await SendMessageAsync(store.TelegramBotToken, telegramMessage.Chat.Id.ToString(), answer, cancellationToken);
    }

    public async Task SendMessageAsync(string botToken, string chatId, string text, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("telegram");
        var endpoint = $"https://api.telegram.org/bot{botToken}/sendMessage";
        var response = await client.PostAsJsonAsync(endpoint, new { chat_id = chatId, text }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Telegram sendMessage failed: {StatusCode}, payload: {Payload}", response.StatusCode, payload);
        }
    }
}
