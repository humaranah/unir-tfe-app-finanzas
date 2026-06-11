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
    private Guid _idMovimiento;
    private IReadOnlyList<CategoriaItem> _todasLasCategorias = [];

    internal Task CargandoCategoriasTask { get; private set; } = Task.CompletedTask;

    public bool ModoEdicion => _idMovimiento != Guid.Empty;
    public string TituloFormulario => ModoEdicion ? "Editar movimiento" : "Nuevo movimiento";
    public string GuardarMovimientoText => ModoEdicion ? "Guardar cambios" : "Guardar movimiento";

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
    [NotifyCanExecuteChangedFor("GuardarMovimientoCommand")]
    public partial string Concepto { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Establecimiento { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Nota { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneComprobante))]
    [NotifyPropertyChangedFor(nameof(NoTieneComprobante))]
    private ComprobanteResult? _comprobante;

    public bool TieneComprobante => Comprobante is not null;
    public bool NoTieneComprobante => Comprobante is null;

    [ObservableProperty]
    public partial bool MostrarOpcionales { get; set; } = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor("GuardarMovimientoCommand")]
    public partial string ImporteTexto { get; set; } = string.Empty;

    [ObservableProperty]
    public partial KeyValuePair<string, string> MonedaSeleccionada { get; set; }
        = MonedasHelper.DefaultMoneda;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor("GuardarMovimientoCommand")]
    [NotifyPropertyChangedFor(nameof(CategoriasFiltradas))]
    [NotifyPropertyChangedFor(nameof(SinCategorias))]
    public partial TipoMovimiento TipoSeleccionado { get; set; } = TipoMovimiento.Gasto;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor("GuardarMovimientoCommand")]
    public partial DateTime Fecha { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial TimeSpan Hora { get; set; } = TimeSpan.Zero;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor("GuardarMovimientoCommand")]
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

    private bool PuedeGuardar =>
        !string.IsNullOrWhiteSpace(Concepto) &&
        decimal.TryParse(ImporteTexto, out var v) && v > 0 &&
        CategoriaSeleccionada is not null &&
        Fecha != default &&
        Enum.IsDefined(TipoSeleccionado);

    public void Reset()
    {
        _idMovimiento = Guid.Empty;
        Concepto = string.Empty;
        Establecimiento = string.Empty;
        Nota = string.Empty;
        Comprobante = null;
        MostrarOpcionales = false;
        ImporteTexto = string.Empty;
        MonedaSeleccionada = MonedasHelper.DefaultMoneda;
        TipoSeleccionado = TipoMovimiento.Gasto;
        Fecha = DateTime.Today;
        Hora = TimeSpan.Zero;
        CategoriaSeleccionada = null;
        Error = string.Empty;
        OnPropertyChanged(nameof(ModoEdicion));
        OnPropertyChanged(nameof(TituloFormulario));
        OnPropertyChanged(nameof(GuardarMovimientoText));
    }

    public async Task CargarMovimientoAsync(Guid idCuenta, Guid idMovimiento, CancellationToken cancellationToken = default)
    {
        _idMovimiento = idMovimiento;
        IdCuenta = idCuenta;

        IsBusy = true;
        Error = string.Empty;
        try
        {
            await CargandoCategoriasTask;

            var detalle = await movimientosService.GetMovimientoDetalleAsync(idCuenta, idMovimiento, cancellationToken);

            Concepto = detalle.Concepto;
            Establecimiento = detalle.Establecimiento ?? string.Empty;
            Nota = detalle.Nota ?? string.Empty;
            ImporteTexto = detalle.Importe.ToString(System.Globalization.CultureInfo.InvariantCulture);
            TipoSeleccionado = detalle.TipoMovimiento;
            Fecha = detalle.FechaMovimiento.Date;
            Hora = detalle.FechaMovimiento.TimeOfDay;
            MostrarOpcionales = HasOptionalFieldsFilled();
            Comprobante = null;

            var moneda = Monedas.FirstOrDefault(m =>
                string.Equals(m.Key, detalle.Moneda, StringComparison.OrdinalIgnoreCase));
            MonedaSeleccionada = moneda.Equals(default(KeyValuePair<string, string>))
                ? MonedasHelper.DefaultMoneda
                : moneda;

            var categoria = _todasLasCategorias.FirstOrDefault(
                c => c.IdCuentaCategoria == detalle.IdCuentaCategoria);
            CategoriaSeleccionada = categoria;

            OnPropertyChanged(nameof(ModoEdicion));
            OnPropertyChanged(nameof(TituloFormulario));
            OnPropertyChanged(nameof(GuardarMovimientoText));
        }
        catch (Exception ex)
        {
            Error = "No se pudo cargar el movimiento. Inténtalo de nuevo.";
            System.Diagnostics.Debug.WriteLine($"Error al cargar movimiento: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
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

    [RelayCommand(CanExecute = nameof(PuedeGuardar))]
    private async Task GuardarMovimientoAsync(CancellationToken cancellationToken)
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
            if (ModoEdicion)
            {
                await movimientosService.UpdateMovimientoAsync(
                    new UpdateMovimientoDto(
                        _idCuenta,
                        _idMovimiento,
                        Concepto.Trim(),
                        importe,
                        MonedaSeleccionada.Key,
                        TipoSeleccionado,
                        Fecha.Date + Hora,
                        CategoriaSeleccionada!.IdCuentaCategoria,
                        string.IsNullOrWhiteSpace(Establecimiento) ? null : Establecimiento.Trim(),
                        string.IsNullOrWhiteSpace(Nota) ? null : Nota.Trim(),
                        Comprobante?.Bytes,
                        Comprobante?.NombreArchivo,
                        Comprobante?.ContentType),
                    cancellationToken);

                await navigationService.GoBackAsync();
            }
            else
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
                        string.IsNullOrWhiteSpace(Nota) ? null : Nota.Trim(),
                        Comprobante?.Bytes,
                        Comprobante?.NombreArchivo,
                        Comprobante?.ContentType),
                    cancellationToken);

                await navigationService.GoToAsync("//movimientos");
            }
        }
        catch (Exception ex)
        {
            Error = ModoEdicion
                ? "No se pudo actualizar el movimiento. Inténtalo de nuevo."
                : "No se pudo registrar el movimiento. Inténtalo de nuevo.";
            System.Diagnostics.Debug.WriteLine($"Error al guardar movimiento: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenComprobanteOptionsAsync(CancellationToken cancellationToken)
    {
        var result = await SelectComprobanteAsync(cancellationToken);
        if (result is not null)
            Comprobante = result;
    }

    private async Task<ComprobanteResult?> SelectComprobanteAsync(CancellationToken cancellationToken)
    {
        var option = await navigationService.DisplayActionSheetAsync(
            "Adjuntar comprobante", "Cancelar", null,
            "Seleccionar archivo", "Tomar foto");

        try
        {
            return option switch
            {
                "Seleccionar archivo" => await comprobantePickerService.SeleccionarArchivoAsync(cancellationToken),
                "Tomar foto" => await comprobantePickerService.TomarFotoAsync(cancellationToken),
                _ => null
            };
        }
        catch (InvalidOperationException ex)
        {
            Error = ex.Message;
            return null;
        }
    }

    [RelayCommand]
    private async Task AttachFileAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await comprobantePickerService.SeleccionarArchivoAsync(cancellationToken);
            if (result is not null)
                Comprobante = result;
        }
        catch (InvalidOperationException ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private async Task TakePhotoAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await comprobantePickerService.TomarFotoAsync(cancellationToken);
            if (result is not null)
                Comprobante = result;
        }
        catch (InvalidOperationException ex)
        {
            Error = ex.Message;
        }
    }

    [RelayCommand]
    private void RemoveComprobante() => Comprobante = null;

    [RelayCommand]
    private async Task ScanComprobanteAsync(CancellationToken cancellationToken)
    {
        var comprobante = await SelectComprobanteAsync(cancellationToken);
        if (comprobante is null)
            return;

        Error = string.Empty;
        IsBusy = true;
        try
        {
            var data = await movimientosService.EscanearComprobanteAsync(IdCuenta, comprobante, cancellationToken);
            FillFromComprobante(data);
            Comprobante = comprobante;
        }
        catch (Exception ex)
        {
            Error = "No se pudo escanear el comprobante. Inténtalo de nuevo.";
            System.Diagnostics.Debug.WriteLine($"Error al escanear comprobante: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FillFromComprobante(ComprobanteExtraidoDto data)
    {
        if (Enum.TryParse<TipoMovimiento>(data.TipoMovimiento, ignoreCase: true, out var tipo))
            TipoSeleccionado = tipo;

        SetIfNotEmpty(data.Concepto, v => Concepto = v);
        SetIfNotEmpty(data.Establecimiento, v => Establecimiento = v);
        SetIfNotEmpty(data.Nota, v => Nota = v);

        if (data.Importe is > 0)
            ImporteTexto = data.Importe.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

        var moneda = Monedas.FirstOrDefault(m =>
            string.Equals(m.Key, data.Moneda, StringComparison.OrdinalIgnoreCase));
        if (!moneda.Equals(default(KeyValuePair<string, string>)))
            MonedaSeleccionada = moneda;

        if (data.FechaMovimiento is { } fecha)
        {
            Fecha = fecha.Date;
            Hora = fecha.TimeOfDay;
        }

        var categoria = _todasLasCategorias.FirstOrDefault(c => c.IdCuentaCategoria == data.IdCuentaCategoria);
        if (categoria is not null)
        {
            TipoSeleccionado = categoria.TipoMovimiento;
            CategoriaSeleccionada = categoria;
        }

        MostrarOpcionales = HasOptionalFieldsFilled();
    }

    private static void SetIfNotEmpty(string? value, Action<string> assign)
    {
        if (!string.IsNullOrWhiteSpace(value))
            assign(value);
    }

    private bool HasOptionalFieldsFilled() =>
        !string.IsNullOrWhiteSpace(Establecimiento)
        || !string.IsNullOrWhiteSpace(Nota)
        || Hora != TimeSpan.Zero;

    [RelayCommand]
    private Task CancelAsync() =>
        ModoEdicion
            ? navigationService.GoBackAsync()
            : navigationService.GoToAsync("//movimientos");
}
