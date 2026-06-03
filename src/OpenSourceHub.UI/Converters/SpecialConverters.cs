using OpenSourceHub.Domain.Enums;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OpenSourceHub.UI.Converters;

public class BoolCheckConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "✅" : "❌";
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class FavoriteLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "★ Remove Favorite" : "☆ Add to Favorites";
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class InitialConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var s = value?.ToString();
        return string.IsNullOrEmpty(s) ? "?" : s[0].ToString().ToUpper();
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class NotifIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is NotificationType t ? t switch
        {
            NotificationType.Success => "✅",
            NotificationType.Warning => "⚠️",
            NotificationType.Error => "❌",
            _ => "ℹ️"
        } : "ℹ️";
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class AuthButtonTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "Authenticating..." : "Sign In with GitHub";
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class ZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i) return i == 0 ? Visibility.Visible : Visibility.Collapsed;
        if (value is System.Collections.ICollection c) return c.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
