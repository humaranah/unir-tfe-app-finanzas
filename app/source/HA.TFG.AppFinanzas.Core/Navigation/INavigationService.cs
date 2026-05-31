namespace HA.TFG.AppFinanzas.Core.Navigation;

public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoBackAsync();
    Task<string?> DisplayActionSheetAsync(string title, string cancel, string? destruction, params string[] buttons);
}
