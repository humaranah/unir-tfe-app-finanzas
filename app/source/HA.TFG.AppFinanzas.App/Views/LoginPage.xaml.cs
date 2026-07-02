using HA.TFG.AppFinanzas.Core.Features.Authentication;

namespace HA.TFG.AppFinanzas.App.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(UsuarioViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
