using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace OpenSourceHub.UI.Controls;

public partial class CircularProgressControl : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(CircularProgressControl),
            new PropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty ArcColorProperty =
        DependencyProperty.Register(nameof(ArcColor), typeof(Color), typeof(CircularProgressControl),
            new PropertyMetadata(Color.FromRgb(0, 120, 212), OnColorChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Color ArcColor
    {
        get => (Color)GetValue(ArcColorProperty);
        set => SetValue(ArcColorProperty, value);
    }

    private double _currentAngle;
    private System.Windows.Threading.DispatcherTimer? _animTimer;

    public CircularProgressControl() => InitializeComponent();

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CircularProgressControl ctrl)
            ctrl.AnimateToValue((double)e.NewValue);
    }

    private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CircularProgressControl ctrl)
            ctrl.UpdateArcColor((Color)e.NewValue);
    }

    private void UpdateArcColor(Color color)
    {
        var lighter = Color.FromRgb(
            (byte)Math.Min(255, color.R + 40),
            (byte)Math.Min(255, color.G + 40),
            (byte)Math.Min(255, color.B + 40));
        GradStop1.Color = color;
        GradStop2.Color = lighter;
    }

    private void AnimateToValue(double targetValue)
    {
        var targetAngle = targetValue / 100.0 * 360.0;
        var fromAngle = _currentAngle;

        _animTimer?.Stop();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        const double duration = 800;

        _animTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _animTimer.Tick += (_, _) =>
        {
            double progress = Math.Min(1.0, sw.ElapsedMilliseconds / duration);
            double eased = 1 - Math.Pow(1 - progress, 4);
            double angle = fromAngle + (targetAngle - fromAngle) * eased;

            DrawArc(angle);
            ValueText.Text = ((int)(targetValue * eased)).ToString();

            if (progress >= 1.0)
            {
                _animTimer.Stop();
                _currentAngle = targetAngle;
                ValueText.Text = ((int)targetValue).ToString();
            }
        };
        _animTimer.Start();
    }

    private void DrawArc(double angleDegrees)
    {
        if (angleDegrees <= 0) { ArcPath.Data = null; return; }
        if (angleDegrees >= 360) angleDegrees = 359.99;

        const double cx = 45, cy = 45, r = 41;
        double startRad = -Math.PI / 2;
        double endRad = startRad + angleDegrees * Math.PI / 180;

        double x1 = cx + r * Math.Cos(startRad);
        double y1 = cy + r * Math.Sin(startRad);
        double x2 = cx + r * Math.Cos(endRad);
        double y2 = cy + r * Math.Sin(endRad);

        bool isLargeArc = angleDegrees > 180;

        var figure = new PathFigure(new Point(x1, y1), new PathSegment[]
        {
            new ArcSegment(new Point(x2, y2), new Size(r, r), 0,
                isLargeArc, SweepDirection.Clockwise, true)
        }, false);

        ArcPath.Data = new PathGeometry(new[] { figure });
    }
}
