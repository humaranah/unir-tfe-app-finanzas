using HA.TFG.AppFinanzas.App.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
