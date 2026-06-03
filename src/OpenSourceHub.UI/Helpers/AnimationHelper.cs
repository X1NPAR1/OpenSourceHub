using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OpenSourceHub.UI.Helpers;

public static class AnimationHelper
{
    public static void FadeIn(UIElement element, double durationMs = 200, Action? onComplete = null)
    {
        element.Opacity = 0;
        element.Visibility = Visibility.Visible;
        var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        if (onComplete != null) anim.Completed += (_, _) => onComplete();
        element.BeginAnimation(UIElement.OpacityProperty, anim);
    }

    public static void FadeOut(UIElement element, double durationMs = 200, Action? onComplete = null)
    {
        var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        anim.Completed += (_, _) =>
        {
            element.Visibility = Visibility.Collapsed;
            onComplete?.Invoke();
        };
        element.BeginAnimation(UIElement.OpacityProperty, anim);
    }

    public static void SlideInFromBottom(UIElement element, double fromY = 20, double durationMs = 250)
    {
        if (element.RenderTransform is not TranslateTransform tt)
        {
            tt = new TranslateTransform(0, fromY);
            element.RenderTransform = tt;
        }
        else
        {
            tt.Y = fromY;
        }

        var slideAnim = new DoubleAnimation(fromY, 0, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        element.Opacity = 0;
        element.Visibility = Visibility.Visible;
        tt.BeginAnimation(TranslateTransform.YProperty, slideAnim);
        element.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
    }

    public static void SlideInFromRight(UIElement element, double fromX = 320, double durationMs = 300)
    {
        if (element.RenderTransform is not TranslateTransform tt)
        {
            tt = new TranslateTransform(fromX, 0);
            element.RenderTransform = tt;
        }
        else
        {
            tt.X = fromX;
        }

        var slideAnim = new DoubleAnimation(fromX, 0, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 4 }
        };
        var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(durationMs * 0.6))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        element.Opacity = 0;
        element.Visibility = Visibility.Visible;
        tt.BeginAnimation(TranslateTransform.XProperty, slideAnim);
        element.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
    }

    public static void SlideOutToRight(UIElement element, double toX = 320, double durationMs = 250, Action? onComplete = null)
    {
        if (element.RenderTransform is not TranslateTransform tt)
        {
            tt = new TranslateTransform(0, 0);
            element.RenderTransform = tt;
        }

        var slideAnim = new DoubleAnimation(0, toX, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        var fadeAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        fadeAnim.Completed += (_, _) =>
        {
            element.Visibility = Visibility.Collapsed;
            onComplete?.Invoke();
        };

        tt.BeginAnimation(TranslateTransform.XProperty, slideAnim);
        element.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
    }

    public static void AnimateWidth(FrameworkElement element, double toWidth, double durationMs = 600)
    {
        var anim = new DoubleAnimation(0, toWidth, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5 }
        };
        element.BeginAnimation(FrameworkElement.WidthProperty, anim);
    }

    public static void PulseScale(UIElement element, double scale = 1.05, double durationMs = 120)
    {
        var group = new TransformGroup();
        var st = new ScaleTransform(1, 1, 0.5, 0.5);
        group.Children.Add(st);
        element.RenderTransform = group;
        element.RenderTransformOrigin = new Point(0.5, 0.5);

        var scaleUpX = new DoubleAnimation(1, scale, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
            AutoReverse = true
        };
        st.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUpX);

        var scaleUpY = new DoubleAnimation(1, scale, TimeSpan.FromMilliseconds(durationMs))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
            AutoReverse = true
        };
        st.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUpY);
    }

    public static Storyboard CreateSpinAnimation(RotateTransform rotateTransform)
    {
        var anim = new DoubleAnimation(0, 360, TimeSpan.FromMilliseconds(900))
        {
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = null
        };
        Storyboard.SetTarget(anim, rotateTransform);
        Storyboard.SetTargetProperty(anim, new PropertyPath(RotateTransform.AngleProperty));
        var sb = new Storyboard();
        sb.Children.Add(anim);
        return sb;
    }

    public static void AnimateNumber(Action<int> setter, int from, int to, double durationMs = 800)
    {
        if (from == to) { setter(to); return; }
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        timer.Tick += (_, _) =>
        {
            double progress = Math.Min(1.0, sw.ElapsedMilliseconds / durationMs);
            double eased = 1 - Math.Pow(1 - progress, 4);
            setter((int)(from + (to - from) * eased));
            if (progress >= 1.0)
            {
                timer.Stop();
                setter(to);
            }
        };
        timer.Start();
    }
}
