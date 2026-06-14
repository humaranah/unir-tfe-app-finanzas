using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Models.Enums;
using System.Collections.ObjectModel;

namespace HA.TFG.AppFinanzas.Core.ViewModels;

public partial class CategoriasViewModel(ICuentasService cuentasService) : ObservableObject
{
    private Guid? _idCuenta;

    [ObservableProperty]
    public partial string NombreCuenta { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SinCategorias))]
    [NotifyPropertyChangedFor(nameof(HasCategorias))]
    public partial ObservableCollection<CategoriaFilaItem> Categorias { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(SinCategorias))]
    [NotifyPropertyChangedFor(nameof(HasCategorias))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(SinCategorias))]
    [NotifyPropertyChangedFor(nameof(HasCategorias))]
    public partial string Error { get; set; } = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(Error);
    public bool IsNotBusy => !IsBusy;
    public bool SinCategorias => !IsBusy && !HasError && Categorias.Count == 0;
    public bool HasCategorias => !IsBusy && !HasError && Categorias.Count > 0;

    [RelayCommand]
    public async Task CargarCategoriasAsync(CancellationToken cancellationToken = default)
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
                Categorias = [];
                return;
            }

            var items = await cuentasService.GetCategoriasAsync(idCuenta.Value, cancellationToken);
            Categorias = new ObservableCollection<CategoriaFilaItem>(
                items.OrderBy(c => c.TipoMovimiento).ThenBy(c => c.Nombre)
                     .Select((item, i) => new CategoriaFilaItem(item, i % 2 == 0)));
        }
        catch (Exception ex)
        {
            Categorias = [];
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed record CategoriaFilaItem(CategoriaItem Categoria, bool IsEven)
{
    public Guid IdCuentaCategoria => Categoria.IdCuentaCategoria;
    public string Nombre => Categoria.Nombre;
    public TipoMovimiento TipoMovimiento => Categoria.TipoMovimiento;
}
