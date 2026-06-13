namespace HA.TFG.AppFinanzas.App.Services;

using HA.TFG.AppFinanzas.Core.Services;

public class ConfirmationService : IConfirmationService
{
    public async Task<bool> ConfirmAsync(string title, string message)
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var window = App.Current?.Windows?[0];
            if (window?.Page == null)
                return false;

            return await window.Page.DisplayAlertAsync(title, message, "Sí", "No");
        });
    }
}
