using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Cuentas;
using HA.TFG.AppFinanzas.Core.Recomendaciones;

namespace HA.TFG.AppFinanzas.Core.ViewModels;

public partial class RecomendacionesViewModel(
    ICuentasService cuentasService,
    IRecomendacionesService recomendacionesService) : ObservableObject
{
    private Guid? _idCuenta;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasContent))]
    public partial string Content { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Query { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyCanExecuteChangedFor(nameof(PreguntarCommand))]
    [NotifyCanExecuteChangedFor(nameof(CargarResumenCommand))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string Error { get; set; } = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(Error);
    public bool IsNotBusy => !IsBusy;
    public bool HasContent => !string.IsNullOrEmpty(Content);

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    public async Task CargarResumenAsync(CancellationToken cancellationToken = default)
    {
        await ObtenerRecomendacionAsync(null, cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task PreguntarAsync(CancellationToken cancellationToken = default)
    {
        var query = Query?.Trim();
        if (string.IsNullOrEmpty(query))
            return;

        await ObtenerRecomendacionAsync(query, cancellationToken);
        Query = string.Empty;
    }

    private async Task ObtenerRecomendacionAsync(string? query, CancellationToken cancellationToken)
    {
        Error = string.Empty;
        IsBusy = true;

        try
        {
            if (_idCuenta is null)
            {
                var (idCuenta, _) = await cuentasService.GetDefaultCuentaAsync(cancellationToken);
                _idCuenta = idCuenta;
            }

            if (_idCuenta is null)
            {
                Content = string.Empty;
                Error = "No se encontró una cuenta para generar recomendaciones.";
                return;
            }

            var result = await recomendacionesService.GetRecomendacionAsync(
                _idCuenta.Value, query, cancellationToken);

            Content = result.Content;
        }
        catch (Exception ex)
        {
            Content = string.Empty;
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
