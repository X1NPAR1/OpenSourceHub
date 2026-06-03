using System.Windows;
using System.Windows.Controls;

namespace OpenSourceHub.UI.Controls;

public partial class EmptyStateControl : UserControl
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(EmptyStateControl),
            new PropertyMetadata("📭", (d, e) => ((EmptyStateControl)d).IconText.Text = (string)e.NewValue));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(EmptyStateControl),
            new PropertyMetadata("Nothing here yet", (d, e) => ((EmptyStateControl)d).TitleText.Text = (string)e.NewValue));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(EmptyStateControl),
            new PropertyMetadata("", (d, e) => ((EmptyStateControl)d).SubtitleText.Text = (string)e.NewValue));

    public static readonly DependencyProperty ActionLabelProperty =
        DependencyProperty.Register(nameof(ActionLabel), typeof(string), typeof(EmptyStateControl),
            new PropertyMetadata(null, (d, e) =>
            {
                var ctrl = (EmptyStateControl)d;
                if (e.NewValue is string s && !string.IsNullOrEmpty(s))
                {
                    ctrl.ActionButton.Content = s;
                    ctrl.ActionButton.Visibility = Visibility.Visible;
                }
            }));

    public string Icon { get => (string)GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Subtitle { get => (string)GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }
    public string? ActionLabel { get => (string?)GetValue(ActionLabelProperty); set => SetValue(ActionLabelProperty, value); }

    public event EventHandler? ActionClicked;

    public EmptyStateControl()
    {
        InitializeComponent();
        ActionButton.Click += (_, _) => ActionClicked?.Invoke(this, EventArgs.Empty);
    }
}
