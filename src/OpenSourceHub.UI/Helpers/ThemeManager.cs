using Microsoft.Win32;
using OpenSourceHub.Domain.Enums;
using System.Windows;
using System.Windows.Media;

namespace OpenSourceHub.UI.Helpers;

/// <summary>
/// Runtime theming without restart. The theme-varying brushes are shared
/// <see cref="SolidColorBrush"/> resources; every control references the same
/// instance via StaticResource, so mutating each brush's <c>Color</c> updates
/// the whole UI live — no DynamicResource conversion or dictionary swap needed.
/// </summary>
public static class ThemeManager
{
    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

    private static readonly Dictionary<string, Color> Dark = new()
    {
        ["BackgroundBrush"]    = Hex("#0D0D14"),
        ["SurfaceBrush"]       = Hex("#13131F"),
        ["CardBrush"]          = Hex("#1A1A2E"),
        ["CardHoverBrush"]     = Hex("#1E1E35"),
        ["ElevatedBrush"]      = Hex("#21213A"),
        ["SidebarBrush"]       = Hex("#0A0A14"),
        ["SidebarItemBrush"]   = Hex("#1A1A2E"),
        ["SidebarHoverBrush"]  = Hex("#1E1E35"),
        ["BorderSubtleBrush"]  = Hex("#252545"),
        ["BorderBrush"]        = Hex("#2E2E52"),
        ["BorderStrongBrush"]  = Hex("#3D3D6B"),
        ["TextBrush"]          = Hex("#E2E8F0"),
        ["SubTextBrush"]       = Hex("#94A3B8"),
        ["MutedTextBrush"]     = Hex("#64748B"),
    };

    private static readonly Dictionary<string, Color> Light = new()
    {
        ["BackgroundBrush"]    = Hex("#F4F6FB"),
        ["SurfaceBrush"]       = Hex("#FFFFFF"),
        ["CardBrush"]          = Hex("#FFFFFF"),
        ["CardHoverBrush"]     = Hex("#EEF1F8"),
        ["ElevatedBrush"]      = Hex("#EAEEF6"),
        ["SidebarBrush"]       = Hex("#EFF1F7"),
        ["SidebarItemBrush"]   = Hex("#E2E7F1"),
        ["SidebarHoverBrush"]  = Hex("#E2E7F1"),
        ["BorderSubtleBrush"]  = Hex("#E3E7F0"),
        ["BorderBrush"]        = Hex("#D4DAE6"),
        ["BorderStrongBrush"]  = Hex("#BFC7D6"),
        ["TextBrush"]          = Hex("#1A1F2B"),
        ["SubTextBrush"]       = Hex("#4A5568"),
        ["MutedTextBrush"]     = Hex("#718096"),
    };

    public static void Apply(AppTheme theme)
    {
        CurrentTheme = theme == AppTheme.System ? GetSystemTheme() : theme;
        var palette = CurrentTheme == AppTheme.Light ? Light : Dark;

        var app = Application.Current;
        if (app == null) return;

        foreach (var (key, color) in palette)
        {
            if (app.Resources[key] is SolidColorBrush brush && !brush.IsFrozen)
                brush.Color = color;
        }
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

    private static Color Hex(string hex) => (Color)ColorConverter.ConvertFromString(hex);
}
