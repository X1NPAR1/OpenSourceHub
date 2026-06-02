using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Domain.Interfaces;

namespace OpenSourceHub.Infrastructure.AI;

public class AiServiceFactory
{
    private readonly OpenAiService _openAi;
    private readonly OllamaService _ollama;
    private readonly ISettingsService _settings;

    public AiServiceFactory(OpenAiService openAi, OllamaService ollama, ISettingsService settings)
    {
        _openAi = openAi;
        _ollama = ollama;
        _settings = settings;
    }

    public async Task<IAiService> GetServiceAsync()
    {
        var s = await _settings.GetSettingsAsync();
        return s.AiProvider == AiProvider.Ollama ? _ollama : _openAi;
    }
}
