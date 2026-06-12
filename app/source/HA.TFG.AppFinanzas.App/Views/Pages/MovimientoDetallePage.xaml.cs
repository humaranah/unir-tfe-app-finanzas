using HA.TFG.AppFinanzas.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

[QueryProperty(nameof(IdCuentaParam), "idCuenta")]
[QueryProperty(nameof(IdMovimientoParam), "idMovimiento")]
public partial class MovimientoDetallePage : ContentPage
{
    private readonly MovimientoDetalleViewModel _viewModel;
    private Guid? _idCuenta;
    private Guid? _idMovimiento;

    public string? IdCuentaParam
    {
        set => _idCuenta = Guid.TryParse(value, out var id) ? id : null;
    }

    public string? IdMovimientoParam
    {
        set => _idMovimiento = Guid.TryParse(value, out var id) ? id : null;
    }

    public MovimientoDetallePage(MovimientoDetalleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (!_idCuenta.HasValue || !_idMovimiento.HasValue)
            return;

        _ = _viewModel.CargarDetalleAsync(_idCuenta.Value, _idMovimiento.Value);

        _idCuenta = null;
        _idMovimiento = null;
    }
}
