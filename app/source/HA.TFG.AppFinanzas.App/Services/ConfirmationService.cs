namespace HA.TFG.AppFinanzas.App.Services;

using HA.TFG.AppFinanzas.Core.Services;

public class ConfirmationService : IConfirmationService
{
    public async Task<bool> ConfirmAsync(string title, string message)
    {
        return await App.Current!.MainPage!.DisplayAlert(title, message, "Sí", "No");
    }
}
