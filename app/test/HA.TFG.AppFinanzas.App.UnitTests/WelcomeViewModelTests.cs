using HA.TFG.AppFinanzas.App.ViewModels;

namespace HA.TFG.AppFinanzas.App.UnitTests;

public class WelcomeViewModelTests
{
    [Fact]
    public void IncrementCounterCommand_UpdatesCounterText()
    {
        var viewModel = new WelcomeViewModel();

        viewModel.IncrementCounterCommand.Execute(null);

        Assert.Equal("Clicked 1 time", viewModel.CounterText);
    }
}
