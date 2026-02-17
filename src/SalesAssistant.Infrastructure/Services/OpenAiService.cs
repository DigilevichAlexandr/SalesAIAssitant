using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SalesAssistant.Application.Abstractions;
using SalesAssistant.Domain.Entities;
using SalesAssistant.Infrastructure.Configuration;

namespace SalesAssistant.Infrastructure.Services;

public class OpenAiService : IOpenAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiService> _logger;

    public OpenAiService(IHttpClientFactory httpClientFactory, IOptions<OpenAiOptions> options, ILogger<OpenAiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GenerateReplyAsync(Store store, Conversation conversation, string userMessage, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("openai");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var payload = new
        {
            model = _options.Model,
            messages = new object[]
            {
                new { role = "system", content = store.SystemPrompt },
                new { role = "user", content = userMessage }
            }
        };

        var response = await client.PostAsync(
            "/chat/completions",
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OpenAI request failed: {Status} - {Error}", response.StatusCode, error);
            return "Sorry, I cannot answer right now. Please try again in a moment.";
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var content = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return string.IsNullOrWhiteSpace(content)
            ? "Could you please clarify your request?"
            : content;
    }
}
