using Microsoft.Extensions.Logging;
using OpenSourceHub.Domain.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace OpenSourceHub.Infrastructure.AI;

public class OllamaService : AiServiceBase
{
    private readonly ISettingsService _settings;
    private readonly HttpClient _http;
    private readonly ILogger<OllamaService> _logger;
    private bool? _available;

    public override bool IsAvailable => _available ?? true; // optimistic until first call proves otherwise

    public OllamaService(ISettingsService settings, HttpClient http, ILogger<OllamaService> logger)
    {
        _settings = settings;
        _http = http;
        _logger = logger;
    }

    protected override async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        try
        {
            var settings = await _settings.GetSettingsAsync();
            var endpoint = settings.OllamaEndpoint.TrimEnd('/');
            var model = string.IsNullOrWhiteSpace(settings.OllamaModel) ? "llama3.2" : settings.OllamaModel;

            var request = new { model, prompt = $"{systemPrompt}\n\n{userPrompt}", stream = false };
            var response = await _http.PostAsJsonAsync($"{endpoint}/api/generate", request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            _available = true;
            return doc.RootElement.GetProperty("response").GetString() ?? "No response from Ollama.";
        }
        catch (Exception ex)
        {
            _available = false;
            _logger.LogError(ex, "Ollama API call failed");
            return $"Ollama not available: {ex.Message}";
        }
    }
}
