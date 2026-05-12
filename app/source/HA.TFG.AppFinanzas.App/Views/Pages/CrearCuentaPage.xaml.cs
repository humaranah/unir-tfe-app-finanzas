using HA.TFG.AppFinanzas.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

public partial class CrearCuentaPage : ContentPage
{
	public CrearCuentaPage(CrearCuentaViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
