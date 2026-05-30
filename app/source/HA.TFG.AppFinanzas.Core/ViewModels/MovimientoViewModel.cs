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
    INavigationService navigationService,
    IComprobantePickerService comprobantePickerService) : ObservableObject
{
    private Guid _idCuenta;
    private IReadOnlyList<CategoriaItem> _todasLasCategorias = [];

    internal Task CargandoCategoriasTask { get; private set; } = Task.CompletedTask;

    public Guid IdCuenta
    {
        get => _idCuenta;
        set
        {
            _idCuenta = value;
            CargandoCategoriasTask = CargarCategoriasAsync();
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CrearMovimientoCommand))]
    public partial string Concepto { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Establecimiento { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Notas { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneComprobante))]
    [NotifyPropertyChangedFor(nameof(NoTieneComprobante))]
    private ComprobanteResult? _comprobante;

    public bool TieneComprobante => Comprobante is not null;
    public bool NoTieneComprobante => Comprobante is null;

    [ObservableProperty]
    public partial bool MostrarOpcionales { get; set; } = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CrearMovimientoCommand))]
    public partial string ImporteTexto { get; set; } = string.Empty;

    [ObservableProperty]
    public partial KeyValuePair<string, string> MonedaSeleccionada { get; set; }
        = MonedasHelper.DefaultMoneda;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CrearMovimientoCommand))]
    [NotifyPropertyChangedFor(nameof(CategoriasFiltradas))]
    [NotifyPropertyChangedFor(nameof(SinCategorias))]
    public partial TipoMovimiento TipoSeleccionado { get; set; } = TipoMovimiento.Gasto;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CrearMovimientoCommand))]
    public partial DateTime Fecha { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial TimeSpan Hora { get; set; } = TimeSpan.Zero;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CrearMovimientoCommand))]
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

    public bool SinCategorias => CategoriasFiltradas.Count == 0;

    partial void OnTipoSeleccionadoChanged(TipoMovimiento value)
    {
        CategoriaSeleccionada = null;
    }

    private bool PuedeCrear =>
        !string.IsNullOrWhiteSpace(Concepto) &&
        decimal.TryParse(ImporteTexto, out var v) && v > 0 &&
        CategoriaSeleccionada is not null &&
        Fecha != default &&
        Enum.IsDefined(TipoSeleccionado);

    public void Reset()
    {
        Concepto = string.Empty;
        Establecimiento = string.Empty;
        Notas = string.Empty;
        Comprobante = null;
        MostrarOpcionales = false;
        ImporteTexto = string.Empty;
        MonedaSeleccionada = MonedasHelper.DefaultMoneda;
        TipoSeleccionado = TipoMovimiento.Gasto;
        Fecha = DateTime.Today;
        Hora = TimeSpan.Zero;
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
            OnPropertyChanged(nameof(SinCategorias));
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
                new CreateMovimientoDto(
                    IdCuenta,
                    Concepto.Trim(),
                    importe,
                    MonedaSeleccionada.Key,
                    TipoSeleccionado,
                    Fecha.Date + Hora,
                    CategoriaSeleccionada!.IdCuentaCategoria,
                    string.IsNullOrWhiteSpace(Establecimiento) ? null : Establecimiento.Trim(),
                    string.IsNullOrWhiteSpace(Notas) ? null : Notas.Trim(),
                    Comprobante?.Bytes,
                    Comprobante?.NombreArchivo,
                    Comprobante?.ContentType),
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
    private async Task AbrirOpcionesComprobanteAsync(CancellationToken cancellationToken)
    {
        var opcion = await navigationService.DisplayActionSheetAsync(
            "Adjuntar comprobante", "Cancelar", null,
            "Seleccionar archivo", "Tomar foto");

        if (opcion == "Seleccionar archivo")
            await AdjuntarArchivoAsync(cancellationToken);
        else if (opcion == "Tomar foto")
            await TomarFotoAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task AdjuntarArchivoAsync(CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await comprobantePickerService.SeleccionarArchivoAsync(cancellationToken);
            if (resultado is not null)
                Comprobante = resultado;
        }
        catch (InvalidOperationException ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private async Task TomarFotoAsync(CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await comprobantePickerService.TomarFotoAsync(cancellationToken);
            if (resultado is not null)
                Comprobante = resultado;
        }
        catch (InvalidOperationException ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private void EliminarComprobante() => Comprobante = null;

    [RelayCommand]
    private Task EscanearComprobanteAsync() => Task.CompletedTask;

    [RelayCommand]
    private Task CancelarAsync() => navigationService.GoToAsync("//movimientos");
}
