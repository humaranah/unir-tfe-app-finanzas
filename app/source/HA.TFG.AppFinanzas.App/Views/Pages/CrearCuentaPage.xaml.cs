using HA.TFG.AppFinanzas.Core.Features.Cuentas;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

public partial class CrearCuentaPage : ContentPage
{
	public CrearCuentaPage(CrearCuentaViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
