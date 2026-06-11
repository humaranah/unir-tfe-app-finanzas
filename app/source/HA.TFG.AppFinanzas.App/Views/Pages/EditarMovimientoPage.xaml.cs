using HA.TFG.AppFinanzas.Core.Movimientos;
using HA.TFG.AppFinanzas.Core.ViewModels;
using System.Text.Json;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

[QueryProperty(nameof(MovimientoParam), "movimiento")]
public partial class EditarMovimientoPage : ContentPage
{
    private readonly MovimientoViewModel _viewModel;

    public string? MovimientoParam
    {
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            var movimiento = JsonSerializer.Deserialize<MovimientoItem>(
                Uri.UnescapeDataString(value),
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                });

            if (movimiento is null)
                return;

            _viewModel.IdCuenta = movimiento.IdCuenta;
            _ = _viewModel.CargarMovimientoAsync(movimiento);
        }
    }

    public EditarMovimientoPage(MovimientoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
