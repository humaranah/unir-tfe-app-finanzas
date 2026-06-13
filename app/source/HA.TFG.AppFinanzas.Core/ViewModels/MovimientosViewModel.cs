using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Movimientos;
using HA.TFG.AppFinanzas.Core.Navigation;
using HA.TFG.AppFinanzas.Core.Services;
using System.Collections.ObjectModel;

namespace HA.TFG.AppFinanzas.Core.ViewModels;

public partial class MovimientosViewModel(
    ICuentasService cuentasService,
    IMovimientosService movimientosService,
    INavigationService navigationService,
    IConfirmationService confirmationService) : ObservableObject
{
    private Guid? _idCuenta;

    [ObservableProperty]
    public partial string NombreCuenta { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SinMovimientos))]
    [NotifyPropertyChangedFor(nameof(HasMovimientos))]
    public partial ObservableCollection<MovimientosPorFecha> Movimientos { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(IsAnteriorEnabled))]
    [NotifyPropertyChangedFor(nameof(IsSiguienteEnabled))]
    [NotifyPropertyChangedFor(nameof(SinMovimientos))]
    [NotifyPropertyChangedFor(nameof(HasMovimientos))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(SinMovimientos))]
    [NotifyPropertyChangedFor(nameof(HasMovimientos))]
    public partial string Error { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Mes))]
    [NotifyPropertyChangedFor(nameof(MesAnterior))]
    [NotifyPropertyChangedFor(nameof(MesSiguiente))]
    [NotifyPropertyChangedFor(nameof(IsAnteriorEnabled))]
    [NotifyPropertyChangedFor(nameof(IsSiguienteEnabled))]
    public partial DateTime Fecha { get; set; } = DateTime.Now;

    public string Mes => Fecha.ToString("MMMM, yyyy");
    public string MesAnterior => Fecha.AddMonths(-1).ToString("MMMM");
    public string MesSiguiente => Fecha.AddMonths(1).ToString("MMMM");

    public bool IsAnteriorEnabled => !IsBusy;
    public bool IsSiguienteEnabled => !IsBusy && (Fecha.Year < DateTime.Now.Year
        || (Fecha.Year == DateTime.Now.Year && Fecha.Month < DateTime.Now.Month));

    public bool HasError => !string.IsNullOrEmpty(Error);
    public bool IsNotBusy => !IsBusy;
    public bool SinMovimientos => !IsBusy && !HasError && Movimientos.Count == 0;
    public bool HasMovimientos => !IsBusy && !HasError && Movimientos.Count > 0;

    [RelayCommand]
    private async Task NuevoMovimientoAsync()
    {
        if (_idCuenta is not null)
            await navigationService.GoToAsync($"crear-movimiento?idCuenta={_idCuenta.Value}");
    }

    [RelayCommand]
    private async Task VerDetalleMovimientoAsync(MovimientoItem movimiento)
    {
        await navigationService.GoToAsync(
            $"detalle-movimiento?idCuenta={movimiento.IdCuenta}&idMovimiento={movimiento.IdMovimiento}");
    }

    [RelayCommand]
    private async Task EditarMovimientoAsync(MovimientoItem movimiento)
    {
        await navigationService.GoToAsync(
            $"editar-movimiento?idCuenta={movimiento.IdCuenta}&idMovimiento={movimiento.IdMovimiento}");
    }

    [RelayCommand]
    private async Task EliminarMovimientoAsync(MovimientoItem movimiento)
    {
        var respuesta = await confirmationService.ConfirmAsync(
            "Eliminar movimiento",
            "¿Estás seguro de que deseas eliminar este movimiento?");

        if (!respuesta)
            return;

        try
        {
            IsBusy = true;
            await movimientosService.DeleteMovimientoAsync(
                movimiento.IdCuenta,
                movimiento.IdMovimiento);
            await CargarMovimientosAsync();
        }
        catch (Exception ex)
        {
            Error = "No se pudo eliminar el movimiento. Inténtalo de nuevo.";
            System.Diagnostics.Debug.WriteLine($"Error al eliminar movimiento: {ex}");
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AnteriorAsync()
    {
        Fecha = Fecha.AddMonths(-1);
        await CargarMovimientosAsync();
    }

    [RelayCommand]
    private async Task SiguienteAsync()
    {
        Fecha = Fecha.AddMonths(1);
        await CargarMovimientosAsync();
    }

    [RelayCommand]
    public async Task CargarMovimientosAsync(CancellationToken cancellationToken = default)
    {
        _idCuenta = null;
        Error = string.Empty;
        IsBusy = true;

        try
        {
            var (idCuenta, descripcion) = await cuentasService.GetDefaultCuentaAsync(cancellationToken);
            _idCuenta = idCuenta;
            NombreCuenta = descripcion ?? string.Empty;

            if (idCuenta is null)
            {
                Movimientos = [];
                return;
            }

            var filtros = new GetMovimientosFilters
            {
                FechaDesde = new DateOnly(Fecha.Year, Fecha.Month, 1),
                FechaHasta = new DateOnly(Fecha.Year, Fecha.Month, DateTime.DaysInMonth(Fecha.Year, Fecha.Month))
            };

            var items = await movimientosService.GetMovimientosAsync(idCuenta.Value, filtros, cancellationToken);

            Movimientos = new ObservableCollection<MovimientosPorFecha>(
                items
                    .GroupBy(m => m.FechaMovimiento)
                    .OrderByDescending(g => g.Key)
                    .Select(g => new MovimientosPorFecha(
                        g.Key,
                        g.OrderByDescending(m => m.FechaMovimiento))));
        }
        catch (Exception ex)
        {
            Movimientos = [];
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
