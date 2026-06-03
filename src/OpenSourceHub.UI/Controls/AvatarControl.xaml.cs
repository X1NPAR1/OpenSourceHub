using OpenSourceHub.UI.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace OpenSourceHub.UI.Controls;

public partial class AvatarControl : UserControl
{
    public static readonly DependencyProperty ImageUrlProperty =
        DependencyProperty.Register(nameof(ImageUrl), typeof(string), typeof(AvatarControl),
            new PropertyMetadata(null, OnImageUrlChanged));

    public static readonly DependencyProperty DisplayNameProperty =
        DependencyProperty.Register(nameof(DisplayName), typeof(string), typeof(AvatarControl),
            new PropertyMetadata(null, OnDisplayNameChanged));

    public static readonly DependencyProperty ShowOnlineProperty =
        DependencyProperty.Register(nameof(ShowOnline), typeof(bool), typeof(AvatarControl),
            new PropertyMetadata(false, OnShowOnlineChanged));

    public string? ImageUrl
    {
        get => (string?)GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    public string? DisplayName
    {
        get => (string?)GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public bool ShowOnline
    {
        get => (bool)GetValue(ShowOnlineProperty);
        set => SetValue(ShowOnlineProperty, value);
    }

    public AvatarControl() => InitializeComponent();

    private static void OnImageUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvatarControl ctrl)
            _ = ctrl.LoadImageAsync(e.NewValue as string);
    }

    private static void OnDisplayNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvatarControl ctrl)
            ctrl.UpdateInitials(e.NewValue as string);
    }

    private static void OnShowOnlineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AvatarControl ctrl)
            ctrl.OnlineDot.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateInitials(string? name)
    {
        if (string.IsNullOrEmpty(name)) { InitialsText.Text = "?"; return; }
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        InitialsText.Text = parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
            : name[0].ToString().ToUpper();

        var fontSize = Width switch { < 30 => 10.0, < 50 => 14.0, < 70 => 18.0, _ => 22.0 };
        InitialsText.FontSize = fontSize;
    }

    private async Task LoadImageAsync(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;

        var bitmap = await ImageCacheService.Instance.LoadAsync(url);
        if (bitmap == null) return;

        Dispatcher.Invoke(() =>
        {
            AvatarImage.Source = bitmap;
            FallbackBorder.Visibility = Visibility.Collapsed;
            ImageBorder.Visibility = Visibility.Visible;
            ImageBorder.Opacity = 0;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ImageBorder.BeginAnimation(OpacityProperty, fadeIn);
        });
    }
}
