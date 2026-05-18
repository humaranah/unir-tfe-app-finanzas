using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HA.TFG.AppFinanzas.Core.Authentication;
using HA.TFG.AppFinanzas.Core.Models;

namespace HA.TFG.AppFinanzas.Core.ViewModels;

public partial class UsuarioViewModel(IUsuarioService usuarioService) : ObservableObject
{
    public event EventHandler? LoginSucceeded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string Error { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotAuthenticated))]
    public partial bool IsAuthenticated { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    public bool IsNotBusy => !IsBusy;
    public bool HasError => !string.IsNullOrEmpty(Error);
    public bool IsNotAuthenticated => !IsAuthenticated;

    public async Task TryRestoreSessionAsync(CancellationToken cancellationToken = default)
    {
        var info = await usuarioService.TryRestoreSessionAsync(cancellationToken);
        if (info is not null)
            SetSession(info);
    }

    [RelayCommand]
    public async Task LoginAsync(CancellationToken cancellationToken = default)
    {
        Error = string.Empty;
        IsBusy = true;

        try
        {
            var info = await usuarioService.LoginAsync(cancellationToken);
            if (info is null)
                return;

            SetSession(info);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
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

    [RelayCommand]
    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await usuarioService.LogoutAsync(cancellationToken);
        IsAuthenticated = false;
        Name = string.Empty;
        Email = string.Empty;
    }

    private void SetSession(UsuarioInfo info)
    {
        Name = info.Nombre;
        Email = info.Email;
        IsAuthenticated = true;
    }
}
