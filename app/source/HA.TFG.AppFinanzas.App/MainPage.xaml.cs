using HA.TFG.AppFinanzas.App.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App
{
    public partial class MainPage : ContentPage
    {
        public MainPage(WelcomeViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            if (Shell.Current is AppShell appShell)
                await appShell.LogoutAndReturnToLoginAsync();
        }
    }
}
