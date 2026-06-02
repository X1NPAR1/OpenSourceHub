using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OpenSourceHub.Domain.Enums;
using OpenSourceHub.Infrastructure.Data;
using OpenSourceHub.Infrastructure.Services;

namespace OpenSourceHub.Tests.Services;

public class SettingsServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task GetSettings_ReturnsDefault_WhenNoSettingsExist()
    {
        using var ctx = CreateContext();
        var service = new SettingsService(ctx, Mock.Of<ILogger<SettingsService>>());

        var settings = await service.GetSettingsAsync();

        settings.Should().NotBeNull();
        settings.Theme.Should().Be(AppTheme.System);
        settings.Language.Should().Be(AppLanguage.English);
    }

    [Fact]
    public async Task SaveSettings_PersistsChanges()
    {
        using var ctx = CreateContext();
        var service = new SettingsService(ctx, Mock.Of<ILogger<SettingsService>>());

        var settings = await service.GetSettingsAsync();
        settings.Theme = AppTheme.Dark;
        settings.Language = AppLanguage.Turkish;
        await service.SaveSettingsAsync(settings);

        var reloaded = await service.GetSettingsAsync();
        reloaded.Theme.Should().Be(AppTheme.Dark);
    }

    [Fact]
    public async Task SaveSettings_FiresSettingsChangedEvent()
    {
        using var ctx = CreateContext();
        var service = new SettingsService(ctx, Mock.Of<ILogger<SettingsService>>());
        bool eventFired = false;
        service.SettingsChanged += (_, _) => eventFired = true;

        var settings = await service.GetSettingsAsync();
        await service.SaveSettingsAsync(settings);

        eventFired.Should().BeTrue();
    }

    [Fact]
    public async Task ResetToDefaults_RestoresDefaultValues()
    {
        using var ctx = CreateContext();
        var service = new SettingsService(ctx, Mock.Of<ILogger<SettingsService>>());

        var settings = await service.GetSettingsAsync();
        settings.Theme = AppTheme.Light;
        settings.Language = AppLanguage.Russian;
        await service.SaveSettingsAsync(settings);

        await service.ResetToDefaultsAsync();
        var reset = await service.GetSettingsAsync();

        reset.Theme.Should().Be(AppTheme.System);
    }
}
