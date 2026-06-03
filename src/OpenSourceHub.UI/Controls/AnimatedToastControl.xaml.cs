using OpenSourceHub.Domain.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OpenSourceHub.UI.Controls;

public partial class AnimatedToastControl : UserControl
{
    private System.Windows.Threading.DispatcherTimer? _dismissTimer;

    public event EventHandler? Dismissed;

    public AnimatedToastControl() => InitializeComponent();

    public void Show(string title, string message, NotificationType type, int autoHideMs = 4500)
    {
        TitleText.Text = title;
        MessageText.Text = message;
        ApplyType(type);

        Visibility = Visibility.Visible;

        var slideIn = new DoubleAnimation(380, 0, TimeSpan.FromMilliseconds(340))
        {
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5 }
        };
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        SlideTf.BeginAnimation(TranslateTransform.XProperty, slideIn);
        BeginAnimation(OpacityProperty, fadeIn);

        if (autoHideMs > 0)
        {
            var progressAnim = new DoubleAnimation(356, 0, TimeSpan.FromMilliseconds(autoHideMs))
            {
                EasingFunction = null
            };
            ProgressFill.BeginAnimation(WidthProperty, progressAnim);

            _dismissTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(autoHideMs)
            };
            _dismissTimer.Tick += (_, _) => { _dismissTimer.Stop(); DismissAsync(); };
            _dismissTimer.Start();
        }
    }

    private void ApplyType(NotificationType type)
    {
        var (icon, accentColor, bgColor) = type switch
        {
            NotificationType.Success => ("", Color.FromRgb(16, 185, 129), Color.FromArgb(30, 16, 185, 129)),
            NotificationType.Warning => ("", Color.FromRgb(245, 158, 11), Color.FromArgb(30, 245, 158, 11)),
            NotificationType.Error   => ("", Color.FromRgb(239, 68, 68),  Color.FromArgb(30, 239, 68, 68)),
            _                        => ("", Color.FromRgb(59, 130, 246), Color.FromArgb(30, 59, 130, 246))
        };

        IconText.Text = icon;
        IconText.FontFamily = (FontFamily)FindResource("IconFont");
        IconText.FontSize = 14;
        IconText.Foreground = new SolidColorBrush(accentColor);

        IconBg.Background = new SolidColorBrush(bgColor);
        AccentBar.Background = new SolidColorBrush(accentColor);

        AccentBar.Effect = new System.Windows.Media.Effects.DropShadowEffect
        {
            BlurRadius = 6,
            ShadowDepth = 0,
            Color = accentColor,
            Opacity = 0.8
        };

        ProgressFill.Background = new SolidColorBrush(accentColor);
        ToastBorder.BorderBrush = new SolidColorBrush(
            Color.FromArgb(80, accentColor.R, accentColor.G, accentColor.B));
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _dismissTimer?.Stop();
        ProgressFill.BeginAnimation(WidthProperty, null);
        DismissAsync();
    }

    private void DismissAsync()
    {
        var slideOut = new DoubleAnimation(0, 380, TimeSpan.FromMilliseconds(260))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        fadeOut.Completed += (_, _) =>
        {
            Visibility = Visibility.Collapsed;
            Dismissed?.Invoke(this, EventArgs.Empty);
        };

        SlideTf.BeginAnimation(TranslateTransform.XProperty, slideOut);
        BeginAnimation(OpacityProperty, fadeOut);
    }
}
