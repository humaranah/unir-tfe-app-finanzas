using HA.TFG.AppFinanzas.Core.Models.Enums;
using HA.TFG.AppFinanzas.Core.Features.Cuentas;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

[QueryProperty(nameof(IdCuentaParam), "idCuenta")]
[QueryProperty(nameof(IdCuentaCategoriaParam), "idCuentaCategoria")]
[QueryProperty(nameof(NombreParam), "nombre")]
[QueryProperty(nameof(TipoParam), "tipo")]
public partial class CategoriaFormPage : ContentPage
{
    private readonly CategoriaFormViewModel _viewModel;
    private Guid? _idCuenta;
    private Guid? _idCuentaCategoria;
    private string? _nombre;
    private TipoMovimiento _tipo = TipoMovimiento.Gasto;

    public string? IdCuentaParam
    {
        set => _idCuenta = Guid.TryParse(value, out var id) ? id : null;
    }

    public string? IdCuentaCategoriaParam
    {
        set => _idCuentaCategoria = Guid.TryParse(value, out var id) ? id : null;
    }

    public string? NombreParam
    {
        set => _nombre = Uri.UnescapeDataString(value ?? string.Empty);
    }

    public string? TipoParam
    {
        set => _tipo = Enum.TryParse<TipoMovimiento>(value, out var t) ? t : TipoMovimiento.Gasto;
    }

    public CategoriaFormPage(CategoriaFormViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (_idCuenta.HasValue)
            _viewModel.Initialize(_idCuenta.Value, _idCuentaCategoria, _nombre, _tipo);
    }
}
