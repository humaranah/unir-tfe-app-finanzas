using HA.TFG.AppFinanzas.Core.ViewModels;
using Indiko.Maui.Controls.Markdown.Theming;

namespace HA.TFG.AppFinanzas.App.Views.Pages;

public partial class RecomendacionesPage : ContentPage
{
    private readonly RecomendacionesViewModel _viewModel;
    private bool _resumenCargado;

    public RecomendacionesPage(RecomendacionesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        markdownView.Theme = MarkdownThemeDefaults.DotNetPurple;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_resumenCargado)
            return;

        _resumenCargado = true;
        await _viewModel.CargarResumenAsync();
    }
}
