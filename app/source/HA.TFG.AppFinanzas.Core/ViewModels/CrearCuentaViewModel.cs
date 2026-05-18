using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Utilities;

namespace HA.TFG.AppFinanzas.Core.ViewModels;

public partial class CrearCuentaViewModel(ICuentasService cuentasService) : ObservableObject
{
    private readonly ICuentasService _cuentasService = cuentasService;

    [ObservableProperty]
    public partial string Descripcion { get; set; } = "Mis gastos";

    [ObservableProperty]
    public partial KeyValuePair<string, string> MonedaSeleccionada { get; set; }
        = MonedasHelper.DefaultMoneda;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string Error { get; set; } = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(Error);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; } = false;

    public bool IsNotBusy => !IsBusy;

    public IReadOnlyList<KeyValuePair<string, string>> Monedas { get; } = MonedasHelper.Monedas;

    public event EventHandler? CuentaCreada;

    [RelayCommand]
    private async Task CrearCuentaAsync(CancellationToken cancellationToken)
    {
        Error = string.Empty;
        IsBusy = true;
        try
        {
            await _cuentasService.CreateCuentaAsync(Descripcion, MonedaSeleccionada.Key, cancellationToken);
            CuentaCreada?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Error = $"No se pudo crear la cuenta. Inténtalo de nuevo.";
            System.Diagnostics.Debug.WriteLine($"Error al crear cuenta: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
