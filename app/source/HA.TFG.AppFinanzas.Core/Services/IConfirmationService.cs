namespace HA.TFG.AppFinanzas.Core.Services;

public interface IConfirmationService
{
    Task<bool> ConfirmAsync(string title, string message);
}
