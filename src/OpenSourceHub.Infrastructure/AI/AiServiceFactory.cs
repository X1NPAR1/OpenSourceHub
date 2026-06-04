using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Domain.Interfaces;

namespace OpenSourceHub.Infrastructure.AI;

public class AiServiceFactory
{
    private readonly OpenAiService _openAi;
    private readonly OllamaService _ollama;
    private readonly ClaudeService _claude;
    private readonly GeminiService _gemini;
    private readonly DeepSeekService _deepSeek;
    private readonly ISettingsService _settings;

    public AiServiceFactory(OpenAiService openAi, OllamaService ollama, ClaudeService claude,
        GeminiService gemini, DeepSeekService deepSeek, ISettingsService settings)
    {
        _openAi = openAi;
        _ollama = ollama;
        _claude = claude;
        _gemini = gemini;
        _deepSeek = deepSeek;
        _settings = settings;
    }

    public async Task<IAiService> GetServiceAsync()
    {
        var s = await _settings.GetSettingsAsync();
        return s.AiProvider switch
        {
            AiProvider.Ollama => _ollama,
            AiProvider.Claude => _claude,
            AiProvider.Gemini => _gemini,
            AiProvider.DeepSeek => _deepSeek,
            _ => _openAi
        };
    }
}
