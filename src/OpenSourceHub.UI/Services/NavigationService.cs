using System.Windows.Controls;

namespace OpenSourceHub.UI.Services;

public class NavigationService
{
    private Frame? _frame;
    private readonly Stack<Page> _history = new();

    public void SetFrame(Frame frame) => _frame = frame;

    public void Navigate(Page page)
    {
        if (_frame == null) return;
        if (_frame.Content is Page current) _history.Push(current);
        _frame.Navigate(page);
    }

    public void GoBack()
    {
        if (_frame == null) return;
        if (_history.Count > 0)
            _frame.Navigate(_history.Pop());
        else if (_frame.CanGoBack)
            _frame.GoBack();
    }

    public bool CanGoBack => _history.Count > 0 || (_frame?.CanGoBack ?? false);
    public string? CurrentPageType => _frame?.Content?.GetType().Name;
}
