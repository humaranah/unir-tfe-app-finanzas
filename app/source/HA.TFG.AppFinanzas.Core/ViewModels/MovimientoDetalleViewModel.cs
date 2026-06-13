using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Movimientos;
using HA.TFG.AppFinanzas.Core.Navigation;
using HA.TFG.AppFinanzas.Core.Services;

namespace HA.TFG.AppFinanzas.Core.ViewModels;

public partial class MovimientoDetalleViewModel(
    IMovimientosService movimientosService,
    ICuentasService cuentasService,
    INavigationService navigationService,
    IComprobanteViewerService comprobanteViewerService,
    IConfirmationService confirmationService) : ObservableObject
{
    private Guid _idCuenta;
    private Guid _idMovimiento;
    private bool _isLoaded;

    [ObservableProperty]
    public partial string Concepto { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Establecimiento { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Nota { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ImporteFormateado))]
    public partial decimal Importe { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ImporteFormateado))]
    public partial string Moneda { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ImporteFormateado))]
    public partial TipoMovimiento Tipo { get; set; }

    [ObservableProperty]
    public partial string NombreCategoria { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FechaFormateada))]
    [NotifyPropertyChangedFor(nameof(HoraFormateada))]
    [NotifyPropertyChangedFor(nameof(MostrarHora))]
    public partial DateTime FechaMovimiento { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(HasDetalle))]
    public partial string Error { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(HasDetalle))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool TieneEstablecimiento { get; set; }

    [ObservableProperty]
    public partial bool TieneNota { get; set; }

    [ObservableProperty]
    public partial bool TieneComprobante { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusyComprobante))]
    [NotifyCanExecuteChangedFor(nameof(VerComprobanteCommand))]
    public partial bool IsBusyComprobante { get; set; }

    public bool IsNotBusyComprobante => !IsBusyComprobante;

    public bool HasError => !string.IsNullOrEmpty(Error);
    public bool IsNotBusy => !IsBusy;
    public bool HasDetalle => !IsBusy && !HasError && _isLoaded;

    public string FechaFormateada => FechaMovimiento.ToString("dd/MM/yyyy");
    public string HoraFormateada => FechaMovimiento.ToString("HH:mm");
    public bool MostrarHora => FechaMovimiento.TimeOfDay != TimeSpan.Zero;

    public string ImporteFormateado => Tipo switch
    {
        TipoMovimiento.Ingreso => $"+{Importe:N2} {Moneda}",
        TipoMovimiento.Gasto => $"-{Importe:N2} {Moneda}",
        _ => $"{Importe:N2} {Moneda}"
    };

    public async Task CargarDetalleAsync(Guid idCuenta, Guid idMovimiento, CancellationToken cancellationToken = default)
    {
        _idCuenta = idCuenta;
        _idMovimiento = idMovimiento;
        _isLoaded = false;
        Error = string.Empty;
        IsBusy = true;
        try
        {
            var detalle = await movimientosService.GetMovimientoDetalleAsync(idCuenta, idMovimiento, cancellationToken);
            var categorias = await cuentasService.GetCategoriasAsync(idCuenta, cancellationToken);

            Concepto = detalle.Concepto;
            Importe = detalle.Importe;
            Moneda = detalle.Moneda;
            Tipo = detalle.TipoMovimiento;
            FechaMovimiento = detalle.FechaMovimiento;
            Establecimiento = detalle.Establecimiento ?? string.Empty;
            Nota = detalle.Nota;
            TieneEstablecimiento = !string.IsNullOrWhiteSpace(Establecimiento);
            TieneNota = !string.IsNullOrWhiteSpace(Nota);
            TieneComprobante = detalle.TieneComprobante;

            var categoria = categorias.FirstOrDefault(c => c.IdCuentaCategoria == detalle.IdCuentaCategoria);
            NombreCategoria = categoria?.Nombre ?? string.Empty;
            _isLoaded = true;
        }
        catch (Exception ex)
        {
            Error = "No se pudo cargar el detalle del movimiento. Inténtalo de nuevo.";
            System.Diagnostics.Debug.WriteLine($"Error al cargar detalle de movimiento: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotBusyComprobante))]
    private async Task VerComprobanteAsync(CancellationToken cancellationToken)
    {
        IsBusyComprobante = true;
        try
        {
            var comprobante = await movimientosService.GetComprobanteAsync(_idCuenta, _idMovimiento, cancellationToken);
            if (comprobante is not null)
                await comprobanteViewerService.AbrirComprobanteAsync(comprobante, cancellationToken);
        }
        catch (Exception ex)
        {
            Error = "No se pudo abrir el comprobante. Inténtalo de nuevo.";
            System.Diagnostics.Debug.WriteLine($"Error al abrir comprobante: {ex}");
        }
        finally
        {
            IsBusyComprobante = false;
        }
    }

    [RelayCommand]
    private async Task EditarMovimientoAsync()
    {
        await navigationService.GoToAsync(
            $"editar-movimiento?idCuenta={_idCuenta}&idMovimiento={_idMovimiento}");
    }

    [RelayCommand]
    private async Task EliminarMovimientoAsync()
    {
        var respuesta = await confirmationService.ConfirmAsync(
            "Eliminar movimiento",
            "¿Estás seguro de que deseas eliminar este movimiento?");

        if (!respuesta)
            return;

        IsBusy = true;
        try
        {
            await movimientosService.DeleteMovimientoAsync(_idCuenta, _idMovimiento);
            await navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            Error = "No se pudo eliminar el movimiento. Inténtalo de nuevo.";
            System.Diagnostics.Debug.WriteLine($"Error al eliminar movimiento: {ex}");
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task VolverAsync() => navigationService.GoBackAsync();
}
