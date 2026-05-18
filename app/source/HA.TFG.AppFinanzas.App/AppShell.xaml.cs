using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App
{
    public partial class AppShell : Shell
    {
        private readonly UsuarioViewModel _usuarioViewModel;
        private readonly ICuentasService _cuentasService;

        public AppShell(UsuarioViewModel usuarioViewModel, ICuentasService cuentasService, CrearCuentaViewModel crearCuentaViewModel)
        {
            InitializeComponent();
            _usuarioViewModel = usuarioViewModel;
            _cuentasService = cuentasService;
            _usuarioViewModel.LoginSucceeded += OnLoginSucceeded;
            crearCuentaViewModel.CuentaCreada += OnCuentaCreada;
            BtnCerrarSesion.Clicked += async (_, _) => await LogoutAndReturnToLoginAsync();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            FlyoutBehavior = width > 1200 ? FlyoutBehavior.Locked : FlyoutBehavior.Flyout;
        }

        protected override async void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            if (Handler is null)
                return;

            if (Window is not null)
            {
                Window.SizeChanged += (_, _) => ActualizarFlyout(Window.Width);
                ActualizarFlyout(Window.Width);
            }

            await EvaluateSessionAsync();
        }

        private async Task EvaluateSessionAsync()
        {
            try
            {
                await _usuarioViewModel.TryRestoreSessionAsync();

                if (!_usuarioViewModel.IsAuthenticated)
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
                await MainThread.InvokeOnMainThreadAsync(() => GoToAsync("//movimientos"));
            }
        }

        private async void OnCuentaCreada(object? sender, EventArgs e)
        {
            await MainThread.InvokeOnMainThreadAsync(() => GoToAsync("//movimientos"));
        }

        private async Task<string> ResolverDestinoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var tieneCuentas = await _cuentasService.HaveCuentasAsync(cancellationToken);
                return tieneCuentas ? "//movimientos" : "//crear-cuenta";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar cuentas: {ex}");
                return "//movimientos";
            }
        }

        public async Task LogoutAndReturnToLoginAsync()
        {
            await _usuarioViewModel.LogoutCommand.ExecuteAsync(null);
            await GoToAsync("//login");
        }

        private void ActualizarFlyout(double width)
        {
            FlyoutBehavior = width > 1200 ? FlyoutBehavior.Locked : FlyoutBehavior.Flyout;
        }
    }
}
