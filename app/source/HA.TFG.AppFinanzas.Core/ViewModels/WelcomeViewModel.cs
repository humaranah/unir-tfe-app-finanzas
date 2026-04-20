using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HA.TFG.AppFinanzas.App.ViewModels;

public partial class WelcomeViewModel : ObservableObject
{
    private int _count;

    public string WelcomeTitle { get; } = "Hello, World!";

    public string WelcomeSubtitle { get; } = "Welcome to\n.NET Multi-platform App UI";

    [ObservableProperty]
    private string counterText = "Click me";

    [RelayCommand]
    private void IncrementCounter()
    {
        _count++;
        CounterText = _count == 1
            ? $"Clicked {_count} time"
            : $"Clicked {_count} times";
    }
}
