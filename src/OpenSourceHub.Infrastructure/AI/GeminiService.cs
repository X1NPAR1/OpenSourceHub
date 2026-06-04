using Microsoft.Extensions.Logging;
using OpenSourceHub.Domain.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace OpenSourceHub.Infrastructure.AI;

/// <summary>Google Gemini via the Generative Language API (generativelanguage.googleapis.com).</summary>
public class GeminiService : AiServiceBase
{
    private readonly ISettingsService _settings;
    private readonly HttpClient _http;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(ISettingsService settings, HttpClient http, ILogger<GeminiService> logger)
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
            return !string.IsNullOrWhiteSpace(s.GeminiApiKey);
        }
    }

    protected override async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        try
        {
            var s = await _settings.GetSettingsAsync();
            if (string.IsNullOrWhiteSpace(s.GeminiApiKey))
                return "Gemini API key not configured. Please set it in Settings → AI.";

            var model = string.IsNullOrWhiteSpace(s.GeminiModel) ? "gemini-2.0-flash" : s.GeminiModel;
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={s.GeminiApiKey}";

            var body = new
            {
                systemInstruction = new { parts = new[] { new { text = systemPrompt } } },
                contents = new[] { new { role = "user", parts = new[] { new { text = userPrompt } } } }
            };

            var response = await _http.PostAsJsonAsync(url, body, ct);
            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                return $"Gemini API error ({(int)response.StatusCode}): {ExtractError(json)}";

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                      .GetProperty("candidates")[0]
                      .GetProperty("content")
                      .GetProperty("parts")[0]
                      .GetProperty("text").GetString()
                   ?? "No response from Gemini.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API call failed");
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
