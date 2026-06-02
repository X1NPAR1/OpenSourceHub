using CommunityToolkit.Mvvm.ComponentModel;
using OpenSourceHub.UI.Services;

namespace OpenSourceHub.UI.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    protected readonly NotificationService Notifications = NotificationService.Instance;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    protected void SetLoading(bool loading, string? status = null)
    {
        IsLoading = loading;
        StatusMessage = status;
        if (!loading) ErrorMessage = null;
    }

    protected void SetError(string message)
    {
        IsLoading = false;
        ErrorMessage = message;
        Notifications.Error(message);
    }

    protected void ClearError()
    {
        ErrorMessage = null;
    }
}
