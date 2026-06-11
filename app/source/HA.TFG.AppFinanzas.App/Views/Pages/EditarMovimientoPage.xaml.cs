using HA.TFG.AppFinanzas.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

[QueryProperty(nameof(IdCuentaParam), "idCuenta")]
[QueryProperty(nameof(IdMovimientoParam), "idMovimiento")]
public partial class EditarMovimientoPage : ContentPage
{
    private readonly MovimientoViewModel _viewModel;
    private Guid? _idCuenta;
    private Guid? _idMovimiento;

    public string? IdCuentaParam
    {
        set
        {
            if (Guid.TryParse(value, out var id))
            {
                _idCuenta = id;
                TryCargar();
            }
        }
    }

    public string? IdMovimientoParam
    {
        set
        {
            if (Guid.TryParse(value, out var id))
            {
                _idMovimiento = id;
                TryCargar();
            }
        }
    }

    private void TryCargar()
    {
        if (_idCuenta.HasValue && _idMovimiento.HasValue)
            _ = _viewModel.CargarMovimientoAsync(_idCuenta.Value, _idMovimiento.Value);
    }

    public EditarMovimientoPage(MovimientoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
