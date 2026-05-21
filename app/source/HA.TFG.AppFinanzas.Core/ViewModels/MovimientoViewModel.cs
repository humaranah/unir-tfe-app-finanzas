using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Movimientos;
using HA.TFG.AppFinanzas.Core.Navigation;
using HA.TFG.AppFinanzas.Core.Utilities;

namespace HA.TFG.AppFinanzas.Core.ViewModels;

public partial class MovimientoViewModel(
    ICuentasService cuentasService,
    IMovimientosService movimientosService,
    INavigationService navigationService) : ObservableObject
{
    private Guid _idCuenta;
    private IReadOnlyList<CategoriaItem> _todasLasCategorias = [];

    public Guid IdCuenta
    {
        get => _idCuenta;
        set
        {
            _idCuenta = value;
            _ = CargarCategoriasAsync();
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CrearMovimientoCommand))]
    public partial string Concepto { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CrearMovimientoCommand))]
    public partial string ImporteTexto { get; set; } = string.Empty;

    [ObservableProperty]
    public partial KeyValuePair<string, string> MonedaSeleccionada { get; set; }
        = MonedasHelper.DefaultMoneda;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CategoriasFiltradas))]
    public partial TipoMovimiento TipoSeleccionado { get; set; } = TipoMovimiento.Gasto;

    [ObservableProperty]
    public partial DateTime Fecha { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial CategoriaItem? CategoriaSeleccionada { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string Error { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool HasError => !string.IsNullOrEmpty(Error);
    public bool IsNotBusy => !IsBusy;

    public IReadOnlyList<KeyValuePair<string, string>> Monedas { get; } = MonedasHelper.Monedas;

    public IReadOnlyList<TipoMovimiento> Tipos { get; } =
        [TipoMovimiento.Gasto, TipoMovimiento.Ingreso, TipoMovimiento.Transferencia];

    public IReadOnlyList<CategoriaItem> CategoriasFiltradas =>
        [.. _todasLasCategorias.Where(c => c.TipoMovimiento == TipoSeleccionado)];

    private bool PuedeCrear =>
        !string.IsNullOrWhiteSpace(Concepto) &&
        decimal.TryParse(ImporteTexto, out var v) && v > 0;

    public void Reset()
    {
        Concepto = string.Empty;
        ImporteTexto = string.Empty;
        MonedaSeleccionada = MonedasHelper.DefaultMoneda;
        TipoSeleccionado = TipoMovimiento.Gasto;
        Fecha = DateTime.Today;
        CategoriaSeleccionada = null;
        Error = string.Empty;
    }

    private async Task CargarCategoriasAsync()
    {
        if (_idCuenta == Guid.Empty) return;
        try
        {
            _todasLasCategorias = await cuentasService.GetCategoriasAsync(_idCuenta);
            OnPropertyChanged(nameof(CategoriasFiltradas));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar categorias: {ex}");
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeCrear))]
    private async Task CrearMovimientoAsync(CancellationToken cancellationToken)
    {
        if (!decimal.TryParse(ImporteTexto, out var importe) || importe <= 0)
        {
            Error = "El importe debe ser un número mayor que cero.";
            return;
        }

        Error = string.Empty;
        IsBusy = true;
        try
        {
            await movimientosService.CreateMovimientoAsync(
                IdCuenta,
                Concepto.Trim(),
                importe,
                MonedaSeleccionada.Key,
                TipoSeleccionado,
                DateOnly.FromDateTime(Fecha),
                CategoriaSeleccionada?.IdCuentaCategoria,
                cancellationToken);

            await navigationService.GoToAsync("//movimientos");
        }
        catch (Exception ex)
        {
            Error = "No se pudo registrar el movimiento. Inténtalo de nuevo.";
            System.Diagnostics.Debug.WriteLine($"Error al crear movimiento: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task CancelarAsync() => navigationService.GoToAsync("//movimientos");
}
