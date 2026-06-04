using Microsoft.Extensions.Logging;
using OpenSourceHub.Domain.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace OpenSourceHub.Infrastructure.AI;

/// <summary>Anthropic Claude via the Messages API (https://api.anthropic.com/v1/messages).</summary>
public class ClaudeService : AiServiceBase
{
    private readonly ISettingsService _settings;
    private readonly HttpClient _http;
    private readonly ILogger<ClaudeService> _logger;

    public ClaudeService(ISettingsService settings, HttpClient http, ILogger<ClaudeService> logger)
    {
        _settings = settings;
        _http = http;
        _logger = logger;
    }

    public override bool IsAvailable
    {
        get
        {
            var s = _settings.GetSettingsAsync().GetAwaiter().GetResult();
            return !string.IsNullOrWhiteSpace(s.ClaudeApiKey);
        }
    }

    protected override async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        try
        {
            var s = await _settings.GetSettingsAsync();
            if (string.IsNullOrWhiteSpace(s.ClaudeApiKey))
                return "Claude API key not configured. Please set it in Settings → AI.";

            var model = string.IsNullOrWhiteSpace(s.ClaudeModel) ? "claude-sonnet-4-5" : s.ClaudeModel;
            var body = new
            {
                model,
                max_tokens = 1500,
                system = systemPrompt,
                messages = new[] { new { role = "user", content = userPrompt } }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            req.Headers.Add("x-api-key", s.ClaudeApiKey);
            req.Headers.Add("anthropic-version", "2023-06-01");
            req.Content = JsonContent.Create(body);

            var response = await _http.SendAsync(req, ct);
            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                return $"Claude API error ({(int)response.StatusCode}): {ExtractError(json)}";

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString()
                   ?? "No response from Claude.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Claude API call failed");
            return $"AI analysis failed: {ex.Message}";
        }
    }

    private static string ExtractError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var err) &&
                err.TryGetProperty("message", out var msg))
                return msg.GetString() ?? json;
        }
        catch { /* fall through */ }
        return json;
    }
}
