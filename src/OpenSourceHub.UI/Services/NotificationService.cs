using OpenSourceHub.Domain.Enums;
using System.Collections.ObjectModel;

namespace OpenSourceHub.UI.Services;

public class NotificationItem
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsVisible { get; set; } = true;
}

public class NotificationService
{
    private static NotificationService? _instance;
    public static NotificationService Instance => _instance ??= new NotificationService();

    public ObservableCollection<NotificationItem> Notifications { get; } = [];

    public void Show(string message, NotificationType type = NotificationType.Information, string? title = null)
    {
        var item = new NotificationItem
        {
            Title = title ?? type.ToString(),
            Message = message,
            Type = type
        };

        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            Notifications.Insert(0, item);
            if (Notifications.Count > 10)
                Notifications.RemoveAt(Notifications.Count - 1);
        });

        Task.Delay(5000).ContinueWith(_ =>
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                item.IsVisible = false;
                Notifications.Remove(item);
            });
        });
    }

    public void Success(string message, string? title = null) => Show(message, NotificationType.Success, title ?? "Success");
    public void Warning(string message, string? title = null) => Show(message, NotificationType.Warning, title ?? "Warning");
    public void Error(string message, string? title = null) => Show(message, NotificationType.Error, title ?? "Error");
    public void Info(string message, string? title = null) => Show(message, NotificationType.Information, title ?? "Information");
}
