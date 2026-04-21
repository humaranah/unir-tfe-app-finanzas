using HA.TFG.AppFinanzas.App.Core.ViewModels;

namespace HA.TFG.AppFinanzas.App
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new WelcomeViewModel();
        }
    }
}
