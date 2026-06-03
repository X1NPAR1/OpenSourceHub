using Microsoft.Win32;
using OpenSourceHub.Domain.Enums;
using System.Windows;

namespace OpenSourceHub.UI.Helpers;

public static class ThemeManager
{
    private static readonly ResourceDictionary _darkDict = new()
    {
        Source = new Uri("pack://application:,,,/Themes/AppTheme.xaml", UriKind.Absolute)
    };
    private static readonly ResourceDictionary _lightDict = new()
    {
        Source = new Uri("pack://application:,,,/Themes/AppTheme.xaml", UriKind.Absolute)
    };

    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

    public static void Apply(AppTheme theme)
    {
        CurrentTheme = theme == AppTheme.System ? GetSystemTheme() : theme;

        var app = System.Windows.Application.Current;
        var mergedDicts = app.Resources.MergedDictionaries;

        var existing = mergedDicts.FirstOrDefault(d =>
            d.Source?.ToString().Contains("Theme.xaml") == true);
        if (existing != null) mergedDicts.Remove(existing);

        var target = CurrentTheme == AppTheme.Light ? _lightDict : _darkDict;
        mergedDicts.Insert(0, target);
    }

    private static AppTheme GetSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int i && i == 1 ? AppTheme.Light : AppTheme.Dark;
        }
        catch
        {
            return AppTheme.Dark;
        }
    }
}
