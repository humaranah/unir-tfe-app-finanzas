using HA.TFG.AppFinanzas.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

[QueryProperty(nameof(IdCuentaParam), "idCuenta")]
public partial class CrearCategoriaPage : ContentPage
{
    private readonly CrearCategoriaViewModel _viewModel;
    private Guid? _idCuenta;

    public string? IdCuentaParam
    {
        set => _idCuenta = Guid.TryParse(value, out var id) ? id : null;
    }

    public CrearCategoriaPage(CrearCategoriaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (_idCuenta.HasValue)
            _viewModel.Initialize(_idCuenta.Value);
    }
}
