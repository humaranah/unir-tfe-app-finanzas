using HA.TFG.AppFinanzas.App.ViewModels;

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
