using HA.TFG.AppFinanzas.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

[QueryProperty(nameof(IdCuentaParam), "idCuenta")]
[QueryProperty(nameof(IdMovimientoParam), "idMovimiento")]
public partial class MovimientoFormPage : ContentPage
{
    private readonly MovimientoViewModel _viewModel;
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

    public MovimientoFormPage(MovimientoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (!_idCuenta.HasValue)
            return;

        _viewModel.IdCuenta = _idCuenta.Value;

        if (_idMovimiento.HasValue)
            _ = _viewModel.CargarMovimientoAsync(_idCuenta.Value, _idMovimiento.Value);
        else
            _viewModel.Reset();

        _idCuenta = null;
        _idMovimiento = null;
    }
}
