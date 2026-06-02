using Microsoft.EntityFrameworkCore;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using OpenSourceHub.Infrastructure.Data;
using System.Text;

namespace OpenSourceHub.Infrastructure.Services;

public class LogService : ILogService
{
    private readonly AppDbContext _db;

    public LogService(AppDbContext db) => _db = db;

    public async Task LogAsync(string level, string category, string message, Exception? exception = null)
    {
        _db.AppLogs.Add(new AppLog
        {
            Level = level,
            Category = category,
            Message = message,
            Exception = exception?.Message,
            StackTrace = exception?.StackTrace,
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<AppLog>> GetLogsAsync(string? level = null, DateTime? from = null, int count = 100)
    {
        var query = _db.AppLogs.AsQueryable();
        if (level != null) query = query.Where(l => l.Level == level);
        if (from.HasValue) query = query.Where(l => l.Timestamp >= from.Value);
        return await query.OrderByDescending(l => l.Timestamp).Take(count).ToListAsync();
    }

    public async Task ClearLogsAsync()
    {
        _db.AppLogs.RemoveRange(_db.AppLogs);
        await _db.SaveChangesAsync();
    }

    public async Task<string> ExportLogsAsync(string outputPath)
    {
        var logs = await GetLogsAsync(count: 10000);
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp\tLevel\tCategory\tMessage\tException");
        foreach (var log in logs)
            sb.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\t{log.Level}\t{log.Category}\t{log.Message}\t{log.Exception}");

        var filePath = Path.Combine(outputPath, $"OpenSourceHub_Logs_{DateTime.Now:yyyyMMdd_HHmmss}.tsv");
        await File.WriteAllTextAsync(filePath, sb.ToString());
        return filePath;
    }
}
