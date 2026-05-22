using HA.TFG.AppFinanzas.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

[QueryProperty(nameof(IdCuentaParam), "idCuenta")]
public partial class CrearMovimientoPage : ContentPage
{
    private readonly MovimientoViewModel _viewModel;

    public string? IdCuentaParam
    {
        set
        {
            if (Guid.TryParse(value, out var id))
            {
                _viewModel.IdCuenta = id;
                _viewModel.Reset();
            }
        }
    }

    public CrearMovimientoPage(MovimientoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
