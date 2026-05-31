using HA.TFG.AppFinanzas.Core.Navigation;

namespace HA.TFG.AppFinanzas.App.Navigation;

internal sealed class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route) =>
        Shell.Current.GoToAsync(route);

    public Task GoBackAsync() =>
        Shell.Current.GoToAsync("..");

    public Task<string?> DisplayActionSheetAsync(string title, string cancel, string? destruction, params string[] buttons) =>
        Shell.Current.DisplayActionSheetAsync(title, cancel, destruction, buttons);
}
