using HA.TFG.AppFinanzas.App.Core.ViewModels;
using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App
{
    public partial class AppShell : Shell
    {
        private readonly WelcomeViewModel _welcomeViewModel;
        private readonly ICuentasService _cuentasService;

        public AppShell(WelcomeViewModel welcomeViewModel, ICuentasService cuentasService, CrearCuentaViewModel crearCuentaViewModel)
        {
            InitializeComponent();
            _welcomeViewModel = welcomeViewModel;
            _cuentasService = cuentasService;
            _welcomeViewModel.LoginSucceeded += OnLoginSucceeded;
            crearCuentaViewModel.CuentaCreada += OnCuentaCreada;
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

                if (!_welcomeViewModel.IsAuthenticated)
                {
                    await GoToAsync("//login");
                    return;
                }

                var destino = await ResolverDestinoAsync();
                await GoToAsync(destino);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Session restore failed: {ex}");
                await GoToAsync("//login");
            }
        }

        private async void OnLoginSucceeded(object? sender, EventArgs e)
        {
            try
            {
                var destino = await ResolverDestinoAsync();
                await MainThread.InvokeOnMainThreadAsync(() => GoToAsync(destino));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Post-login navigation failed: {ex}");
                await MainThread.InvokeOnMainThreadAsync(() => GoToAsync("//home"));
            }
        }

        private async void OnCuentaCreada(object? sender, EventArgs e)
        {
            await MainThread.InvokeOnMainThreadAsync(() => GoToAsync("//home"));
        }

        private async Task<string> ResolverDestinoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var tieneCuentas = await _cuentasService.TieneCuentasAsync(cancellationToken);
                return tieneCuentas ? "//home" : "//crear-cuenta";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar cuentas: {ex}");
                return "//home";
            }
        }

        public async Task LogoutAndReturnToLoginAsync()
        {
            await _welcomeViewModel.LogoutCommand.ExecuteAsync(null);
            await GoToAsync("//login");
        }
    }
}
