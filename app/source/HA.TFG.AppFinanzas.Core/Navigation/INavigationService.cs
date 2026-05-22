namespace HA.TFG.AppFinanzas.Core.Navigation;

public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoBackAsync();
}
