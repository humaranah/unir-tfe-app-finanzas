using HA.TFG.AppFinanzas.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

public partial class MovimientosPage : ContentPage
{
	public MovimientosPage(MovimientosViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is MovimientosViewModel vm)
			await vm.CargarMovimientosAsync();
	}
}
