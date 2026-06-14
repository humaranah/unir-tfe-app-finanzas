using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Navigation;

namespace HA.TFG.AppFinanzas.Core.ViewModels;

public partial class CategoriaFormViewModel(
    ICuentasService cuentasService,
    INavigationService navigationService) : ObservableObject
{
    private Guid _idCuenta;
    private Guid? _idCuentaCategoria;

    public bool EsModoEdicion => _idCuentaCategoria.HasValue;
    public string Titulo => EsModoEdicion ? "Editar categoría" : "Nueva categoría";

    public void Initialize(Guid idCuenta, Guid? idCuentaCategoria = null, string? nombre = null, TipoMovimiento tipo = TipoMovimiento.Gasto)
    {
        _idCuenta = idCuenta;
        _idCuentaCategoria = idCuentaCategoria;
        Nombre = nombre ?? string.Empty;
        TipoSeleccionado = tipo;
        Error = string.Empty;
        OnPropertyChanged(nameof(EsModoEdicion));
        OnPropertyChanged(nameof(Titulo));
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor("GuardarCommand")]
    public partial string Nombre { get; set; } = string.Empty;

    [ObservableProperty]
    public partial TipoMovimiento TipoSeleccionado { get; set; } = TipoMovimiento.Gasto;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string Error { get; set; } = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(Error);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; } = false;

    public bool IsNotBusy => !IsBusy;

    public IReadOnlyList<TipoMovimiento> Tipos { get; } =
        [TipoMovimiento.Gasto, TipoMovimiento.Ingreso];

    [RelayCommand(CanExecute = nameof(CanGuardar))]
    private async Task GuardarAsync(CancellationToken cancellationToken)
    {
        Error = string.Empty;
        IsBusy = true;
        try
        {
            if (EsModoEdicion)
                await cuentasService.UpdateCategoriaAsync(_idCuenta, _idCuentaCategoria!.Value, Nombre, TipoSeleccionado, cancellationToken);
            else
                await cuentasService.CreateCategoriaAsync(_idCuenta, Nombre, TipoSeleccionado, cancellationToken);

            await navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            Error = EsModoEdicion
                ? "No se pudo actualizar la categoría. Inténtalo de nuevo."
                : "No se pudo crear la categoría. Inténtalo de nuevo.";
            System.Diagnostics.Debug.WriteLine($"Error al guardar categoría: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanGuardar() => !string.IsNullOrWhiteSpace(Nombre);
}
