using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Infrastructure.Services;

namespace OpenSourceHub.Tests.Services;

public class SettingsServiceTests
{
    private static SettingsService CreateService()
    {
        var provider = TestDb.NewProvider();
        return new SettingsService(provider.ScopeFactory(), Mock.Of<ILogger<SettingsService>>());
    }

    [Fact]
    public async Task GetSettings_ReturnsDefault_WhenNoSettingsExist()
    {
        var service = CreateService();

        var settings = await service.GetSettingsAsync();

        settings.Should().NotBeNull();
        settings.Theme.Should().Be(AppTheme.System);
        settings.Language.Should().Be(AppLanguage.English);
        settings.AiProvider.Should().Be(AiProvider.OpenAI);
    }

    [Fact]
    public async Task SaveSettings_PersistsChanges()
    {
        var service = CreateService();

        var settings = await service.GetSettingsAsync();
        settings.Theme = AppTheme.Dark;
        settings.Language = AppLanguage.Turkish;
        await service.SaveSettingsAsync(settings);

        var reloaded = await service.GetSettingsAsync();
        reloaded.Theme.Should().Be(AppTheme.Dark);
        reloaded.Language.Should().Be(AppLanguage.Turkish);
    }

    [Fact]
    public async Task SaveSettings_PersistsNewAiProviderFields()
    {
        var service = CreateService();

        var settings = await service.GetSettingsAsync();
        settings.AiProvider = AiProvider.Claude;
        settings.ClaudeApiKey = "sk-ant-test";
        settings.ClaudeModel = "claude-sonnet-4-5";
        await service.SaveSettingsAsync(settings);

        var reloaded = await service.GetSettingsAsync();
        reloaded.AiProvider.Should().Be(AiProvider.Claude);
        reloaded.ClaudeApiKey.Should().Be("sk-ant-test");
        reloaded.ClaudeModel.Should().Be("claude-sonnet-4-5");
    }

    [Fact]
    public async Task SaveSettings_FiresSettingsChangedEvent()
    {
        var service = CreateService();
        bool eventFired = false;
        service.SettingsChanged += (_, _) => eventFired = true;

        var settings = await service.GetSettingsAsync();
        await service.SaveSettingsAsync(settings);

        eventFired.Should().BeTrue();
    }

    [Fact]
    public async Task ResetToDefaults_RestoresDefaultValues()
    {
        var service = CreateService();

        var settings = await service.GetSettingsAsync();
        settings.Theme = AppTheme.Light;
        settings.Language = AppLanguage.Russian;
        await service.SaveSettingsAsync(settings);

        await service.ResetToDefaultsAsync();
        var reset = await service.GetSettingsAsync();

        reset.Theme.Should().Be(AppTheme.System);
        reset.Language.Should().Be(AppLanguage.English);
    }
}
