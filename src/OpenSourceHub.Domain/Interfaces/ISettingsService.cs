using OpenSourceHub.Domain.Entities;

namespace OpenSourceHub.Domain.Interfaces;

public interface ISettingsService
{
    Task<AppSettings> GetSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
    Task ResetToDefaultsAsync();
    event EventHandler<AppSettings> SettingsChanged;
}
