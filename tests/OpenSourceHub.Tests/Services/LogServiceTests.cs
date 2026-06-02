using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OpenSourceHub.Infrastructure.Data;
using OpenSourceHub.Infrastructure.Services;
using System.IO;

namespace OpenSourceHub.Tests.Services;

public class LogServiceTests
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
    public async Task LogAsync_AddsLogEntry()
    {
        using var ctx = CreateContext();
        var service = new LogService(ctx);

        await service.LogAsync("Info", "Test", "Test message");

        var logs = await service.GetLogsAsync();
        logs.Should().HaveCount(1);
        logs[0].Message.Should().Be("Test message");
        logs[0].Level.Should().Be("Info");
    }

    [Fact]
    public async Task LogAsync_WithException_StoresExceptionInfo()
    {
        using var ctx = CreateContext();
        var service = new LogService(ctx);

        var ex = new InvalidOperationException("Test exception");
        await service.LogAsync("Error", "Test", "Error occurred", ex);

        var logs = await service.GetLogsAsync("Error");
        logs.Should().HaveCount(1);
        logs[0].Exception.Should().Contain("Test exception");
    }

    [Fact]
    public async Task ClearLogs_RemovesAllEntries()
    {
        using var ctx = CreateContext();
        var service = new LogService(ctx);

        await service.LogAsync("Info", "Test", "Message 1");
        await service.LogAsync("Warning", "Test", "Message 2");
        await service.ClearLogsAsync();

        var logs = await service.GetLogsAsync();
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLogs_FiltersByLevel()
    {
        using var ctx = CreateContext();
        var service = new LogService(ctx);

        await service.LogAsync("Info", "Test", "Info message");
        await service.LogAsync("Error", "Test", "Error message");
        await service.LogAsync("Warning", "Test", "Warning message");

        var errorLogs = await service.GetLogsAsync("Error");
        errorLogs.Should().HaveCount(1);
        errorLogs[0].Level.Should().Be("Error");
    }

    [Fact]
    public async Task ExportLogs_CreatesFile()
    {
        using var ctx = CreateContext();
        var service = new LogService(ctx);

        await service.LogAsync("Info", "Test", "Export test");

        var tempPath = Path.GetTempPath();
        var filePath = await service.ExportLogsAsync(tempPath);

        File.Exists(filePath).Should().BeTrue();
        File.Delete(filePath);
    }
}
