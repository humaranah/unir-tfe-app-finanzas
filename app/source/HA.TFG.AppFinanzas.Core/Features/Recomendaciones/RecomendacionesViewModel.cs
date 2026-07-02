using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Features.Cuentas;
using HA.TFG.AppFinanzas.Core.Features.Recomendaciones;
using System.Collections.ObjectModel;

namespace HA.TFG.AppFinanzas.Core.Features.Recomendaciones;

public partial class RecomendacionesViewModel(
    ICuentasService cuentasService,
    IRecomendacionesService recomendacionesService) : ObservableObject
{
    private Guid? _idCuenta;

    public ObservableCollection<ChatMessage> Messages { get; } = [];

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
    public bool HasMessages => Messages.Count > 0;

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

        Messages.Add(new ChatMessage(IsUser: true, Content: query));
        Query = string.Empty;

        await ObtenerRecomendacionAsync(query, cancellationToken);
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
                Error = "No se encontró una cuenta para generar recomendaciones.";
                return;
            }

            var result = await recomendacionesService.GetRecomendacionAsync(
                _idCuenta.Value, query, cancellationToken);

            Messages.Add(new ChatMessage(IsUser: false, Content: result.Content));
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
