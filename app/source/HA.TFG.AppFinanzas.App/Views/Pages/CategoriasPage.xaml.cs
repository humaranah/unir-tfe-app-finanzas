using HA.TFG.AppFinanzas.Core.Features.Cuentas;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

public partial class CategoriasPage : ContentPage
{
    private readonly CategoriasViewModel _viewModel;

    public CategoriasPage(CategoriasViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CargarCategoriasAsync();
    }
}
