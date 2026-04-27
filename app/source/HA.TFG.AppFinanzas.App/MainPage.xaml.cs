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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is WelcomeViewModel vm && !vm.IsAuthenticated)
            {
                try
                {
                    await vm.TryRestoreSessionAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to restore session: {ex}");
                }
            }
        }
    }
}
