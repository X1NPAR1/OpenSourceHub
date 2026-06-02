using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceHub.Domain.Entities;
using OpenSourceHub.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace OpenSourceHub.UI.ViewModels;

public partial class LogsViewModel : BaseViewModel
{
    private readonly ILogService _logService;

    [ObservableProperty] private ObservableCollection<AppLog> _logs = [];
    [ObservableProperty] private string _selectedLevel = "All";
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _errorCount;
    [ObservableProperty] private int _warningCount;

    public LogsViewModel(ILogService logService) => _logService = logService;

    [RelayCommand]
    public async Task LoadAsync()
    {
        SetLoading(true);
        try
        {
            var level = SelectedLevel == "All" ? null : SelectedLevel;
            var list = await _logService.GetLogsAsync(level, count: 500);
            Logs = new ObservableCollection<AppLog>(list);
            TotalCount = Logs.Count;
            ErrorCount = Logs.Count(l => l.Level == "Error");
            WarningCount = Logs.Count(l => l.Level == "Warning");
        }
        finally { SetLoading(false); }
    }

    [RelayCommand]
    private async Task FilterAsync(string level)
    {
        SelectedLevel = level;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ClearAsync()
    {
        await _logService.ClearLogsAsync();
        Logs.Clear();
        TotalCount = ErrorCount = WarningCount = 0;
        Notifications.Success("Logs cleared.");
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var file = await _logService.ExportLogsAsync(path);
        Notifications.Success($"Logs exported: {file}");
    }
}
