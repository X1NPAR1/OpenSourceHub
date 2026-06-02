using OpenSourceHub.Domain.Entities;

namespace OpenSourceHub.Domain.Interfaces;

public interface ILogService
{
    Task LogAsync(string level, string category, string message, Exception? exception = null);
    Task<List<AppLog>> GetLogsAsync(string? level = null, DateTime? from = null, int count = 100);
    Task ClearLogsAsync();
    Task<string> ExportLogsAsync(string outputPath);
}
