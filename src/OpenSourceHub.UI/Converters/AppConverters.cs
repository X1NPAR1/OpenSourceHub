using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using OpenSourceHub.Domain.Enums;

namespace OpenSourceHub.UI.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var invert = parameter as string == "invert";
        var isTrue = value is bool b && b;
        return (isTrue ^ invert) ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility.Visible;
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var invert = parameter as string == "invert";
        var isNull = value == null || (value is string s && string.IsNullOrEmpty(s));
        return (isNull ^ invert) ? Visibility.Collapsed : Visibility.Visible;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class NumberToKiloConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i)
        {
            if (i >= 1_000_000) return $"{i / 1_000_000.0:F1}M";
            if (i >= 1000) return $"{i / 1000.0:F1}K";
            return i.ToString();
        }
        return value?.ToString() ?? "0";
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class HealthScoreToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var score = value is double d ? d : 0;
        return score switch
        {
            >= 80 => new SolidColorBrush(Color.FromRgb(16, 124, 16)),
            >= 60 => new SolidColorBrush(Color.FromRgb(0, 120, 212)),
            >= 40 => new SolidColorBrush(Color.FromRgb(255, 140, 0)),
            >= 20 => new SolidColorBrush(Color.FromRgb(220, 53, 69)),
            _ => new SolidColorBrush(Color.FromRgb(168, 0, 0))
        };
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class HealthLevelToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is RepositoryHealthLevel level)
        {
            return level switch
            {
                RepositoryHealthLevel.Excellent => "Excellent",
                RepositoryHealthLevel.Good => "Good",
                RepositoryHealthLevel.Fair => "Fair",
                RepositoryHealthLevel.Poor => "Poor",
                RepositoryHealthLevel.Critical => "Critical",
                _ => level.ToString()
            };
        }
        return string.Empty;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class SecurityRiskToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SecurityRiskLevel level)
        {
            return level switch
            {
                SecurityRiskLevel.Critical => new SolidColorBrush(Color.FromRgb(168, 0, 0)),
                SecurityRiskLevel.High => new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                SecurityRiskLevel.Medium => new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                SecurityRiskLevel.Low => new SolidColorBrush(Color.FromRgb(255, 193, 7)),
                _ => new SolidColorBrush(Color.FromRgb(16, 124, 16))
            };
        }
        return Brushes.Gray;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class DateToRelativeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        DateTime dt = value switch
        {
            DateTime d => d,
            DateTimeOffset dto => dto.DateTime,
            _ => DateTime.UtcNow
        };

        var diff = DateTime.UtcNow - dt.ToUniversalTime();
        if (diff.TotalSeconds < 60) return "just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays < 30) return $"{(int)diff.TotalDays}d ago";
        if (diff.TotalDays < 365) return $"{(int)(diff.TotalDays / 30)}mo ago";
        return $"{(int)(diff.TotalDays / 365)}y ago";
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BytesToMbConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is long kb) return $"{kb / 1024.0:F1} MB";
        return "N/A";
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class StringIsNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.IsNullOrEmpty(value as string);
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}

public class PercentageToWidthConverter : IValueConverter
{
    public static readonly PercentageToWidthConverter Instance = new();
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double score = value is double d ? d : 0;
        double maxWidth = parameter is string s && double.TryParse(s, out var w) ? w : 200;
        return score / 100.0 * maxWidth;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class ListCountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count) return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        if (value is System.Collections.ICollection col) return col.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
