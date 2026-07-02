using HA.TFG.AppFinanzas.Core.Features.Movimientos;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

public partial class MovimientosPage : ContentPage
{
	private readonly MovimientosViewModel _viewModel;

	public MovimientosPage(MovimientosViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.CargarMovimientosAsync();
	}
}
