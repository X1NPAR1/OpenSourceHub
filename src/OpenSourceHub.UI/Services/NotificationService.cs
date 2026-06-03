using OpenSourceHub.Domain.Enums;
using OpenSourceHub.UI.Controls;
using System.Windows;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Services;

public sealed class NotificationService
{
    private static readonly NotificationService _instance = new();
    public static NotificationService Instance => _instance;

    private StackPanel? _container;

    public void SetContainer(StackPanel panel) => _container = panel;

    public void Show(string message, NotificationType type = NotificationType.Information, string? title = null)
    {
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.BeginInvoke(() => Show(message, type, title));
            return;
        }

        if (_container == null) return;

        var toastTitle = title ?? type switch
        {
            NotificationType.Success => "Success",
            NotificationType.Warning => "Warning",
            NotificationType.Error => "Error",
            _ => "Information"
        };

        var toast = new AnimatedToastControl();
        toast.Dismissed += (_, _) =>
        {
            Application.Current.Dispatcher.Invoke(() => _container.Children.Remove(toast));
        };

        _container.Children.Insert(0, toast);
        toast.Show(toastTitle, message, type);

        if (_container.Children.Count > 5)
            _container.Children.RemoveAt(_container.Children.Count - 1);
    }

    public void Success(string message, string? title = null) => Show(message, NotificationType.Success, title);
    public void Warning(string message, string? title = null) => Show(message, NotificationType.Warning, title);
    public void Error(string message, string? title = null) => Show(message, NotificationType.Error, title);
    public void Info(string message, string? title = null) => Show(message, NotificationType.Information, title);
}
