using System.Windows.Data;

namespace OpenSourceHub.Localization;

public class LocalizationExtension : Binding
{
    public LocalizationExtension(string key) : base($"[{key}]")
    {
        Source = LocalizationManager.Instance;
        Mode = BindingMode.OneWay;
    }
}
