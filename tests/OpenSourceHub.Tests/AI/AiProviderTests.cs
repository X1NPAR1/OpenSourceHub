using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Infrastructure.AI;

namespace OpenSourceHub.Tests.AI;

public class AiProviderTests
{
    private static AiServiceFactory BuildFactory(AppSettings settings)
    {
        var settingsService = new Mock<ISettingsService>();
        settingsService.Setup(s => s.GetSettingsAsync()).ReturnsAsync(settings);

        var openAi = new OpenAiService(settingsService.Object, Mock.Of<ILogger<OpenAiService>>());
        var ollama = new OllamaService(settingsService.Object, new HttpClient(), Mock.Of<ILogger<OllamaService>>());
        var claude = new ClaudeService(settingsService.Object, new HttpClient(), Mock.Of<ILogger<ClaudeService>>());
        var gemini = new GeminiService(settingsService.Object, new HttpClient(), Mock.Of<ILogger<GeminiService>>());
        var deepSeek = new DeepSeekService(settingsService.Object, Mock.Of<ILogger<DeepSeekService>>());

        return new AiServiceFactory(openAi, ollama, claude, gemini, deepSeek, settingsService.Object);
    }

    [Theory]
    [InlineData(AiProvider.OpenAI, typeof(OpenAiService))]
    [InlineData(AiProvider.Ollama, typeof(OllamaService))]
    [InlineData(AiProvider.Claude, typeof(ClaudeService))]
    [InlineData(AiProvider.Gemini, typeof(GeminiService))]
    [InlineData(AiProvider.DeepSeek, typeof(DeepSeekService))]
    public async Task Factory_ReturnsCorrectService_ForProvider(AiProvider provider, Type expected)
    {
        var factory = BuildFactory(new AppSettings { AiProvider = provider });

        var service = await factory.GetServiceAsync();

        service.Should().BeOfType(expected);
    }

    [Fact]
    public void ModelCatalog_HasModelsForEveryProvider()
    {
        AiModelCatalog.OpenAi.Should().NotBeEmpty();
        AiModelCatalog.Claude.Should().NotBeEmpty();
        AiModelCatalog.Gemini.Should().NotBeEmpty();
        AiModelCatalog.DeepSeek.Should().NotBeEmpty();
        AiModelCatalog.Ollama.Should().NotBeEmpty();
    }

    [Fact]
    public void Service_IsUnavailable_WhenNoApiKey()
    {
        var settingsService = new Mock<ISettingsService>();
        settingsService.Setup(s => s.GetSettingsAsync()).ReturnsAsync(new AppSettings());
        var claude = new ClaudeService(settingsService.Object, new HttpClient(), Mock.Of<ILogger<ClaudeService>>());

        claude.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Service_IsAvailable_WhenApiKeyPresent()
    {
        var settingsService = new Mock<ISettingsService>();
        settingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(new AppSettings { GeminiApiKey = "test-key" });
        var gemini = new GeminiService(settingsService.Object, new HttpClient(), Mock.Of<ILogger<GeminiService>>());

        gemini.IsAvailable.Should().BeTrue();
    }
}
