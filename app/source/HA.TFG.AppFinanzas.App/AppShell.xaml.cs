using HA.TFG.AppFinanzas.App.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App
{
    public partial class AppShell : Shell
    {
        private readonly WelcomeViewModel _welcomeViewModel;

        public AppShell(WelcomeViewModel welcomeViewModel)
        {
            InitializeComponent();
            _welcomeViewModel = welcomeViewModel;
            _welcomeViewModel.LoginSucceeded += OnLoginSucceeded;
        }

        protected override async void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            if (Handler is null)
                return;

            await EvaluateSessionAsync();
        }

        private async Task EvaluateSessionAsync()
        {
            try
            {
                await _welcomeViewModel.TryRestoreSessionAsync();
                if (_welcomeViewModel.IsAuthenticated)
                    await GoToAsync("//home");
                // Si no está autenticado, permanece en //login (ruta inicial del Shell)
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Session restore failed: {ex}");
            }
        }

        private async void OnLoginSucceeded(object? sender, EventArgs e)
        {
            await MainThread.InvokeOnMainThreadAsync(() => GoToAsync("//home"));
        }

        public async Task LogoutAndReturnToLoginAsync()
        {
            await _welcomeViewModel.LogoutCommand.ExecuteAsync(null);
            await GoToAsync("//login");
        }
    }
}
