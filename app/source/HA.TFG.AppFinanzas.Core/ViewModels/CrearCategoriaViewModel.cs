using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Navigation;

namespace HA.TFG.AppFinanzas.Core.ViewModels;

public partial class CrearCategoriaViewModel(
    ICuentasService cuentasService,
    INavigationService navigationService) : ObservableObject
{
    private Guid _idCuenta;

    public void Initialize(Guid idCuenta)
    {
        _idCuenta = idCuenta;
        Nombre = string.Empty;
        TipoSeleccionado = TipoMovimiento.Gasto;
        Error = string.Empty;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor("CrearCategoriaCommand")]
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

    [RelayCommand(CanExecute = nameof(CanCrearCategoria))]
    private async Task CrearCategoriaAsync(CancellationToken cancellationToken)
    {
        Error = string.Empty;
        IsBusy = true;
        try
        {
            await cuentasService.CreateCategoriaAsync(_idCuenta, Nombre, TipoSeleccionado, cancellationToken);
            await navigationService.GoBackAsync();
        }
        catch (Exception ex)
        {
            Error = "No se pudo crear la categoría. Inténtalo de nuevo.";
            System.Diagnostics.Debug.WriteLine($"Error al crear categoría: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanCrearCategoria() => !string.IsNullOrWhiteSpace(Nombre);
}
