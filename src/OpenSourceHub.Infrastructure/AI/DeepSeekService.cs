using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using OpenSourceHub.Domain.Interfaces;
using System.ClientModel;

namespace OpenSourceHub.Infrastructure.AI;

/// <summary>DeepSeek exposes an OpenAI-compatible API, so we reuse the OpenAI SDK with a custom endpoint.</summary>
public class DeepSeekService : AiServiceBase
{
    private readonly ISettingsService _settings;
    private readonly ILogger<DeepSeekService> _logger;

    public DeepSeekService(ISettingsService settings, ILogger<DeepSeekService> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public override bool IsAvailable
    {
        get
        {
            var s = _settings.GetSettingsAsync().GetAwaiter().GetResult();
            return !string.IsNullOrWhiteSpace(s.DeepSeekApiKey);
        }
    }

    protected override async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        try
        {
            var s = await _settings.GetSettingsAsync();
            if (string.IsNullOrWhiteSpace(s.DeepSeekApiKey))
                return "DeepSeek API key not configured. Please set it in Settings → AI.";

            var options = new OpenAIClientOptions { Endpoint = new Uri("https://api.deepseek.com") };
            var client = new OpenAIClient(new ApiKeyCredential(s.DeepSeekApiKey), options)
                .GetChatClient(string.IsNullOrWhiteSpace(s.DeepSeekModel) ? "deepseek-chat" : s.DeepSeekModel);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            var response = await client.CompleteChatAsync(messages, cancellationToken: ct);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeepSeek API call failed");
            return $"AI analysis failed: {ex.Message}";
        }
    }
}
