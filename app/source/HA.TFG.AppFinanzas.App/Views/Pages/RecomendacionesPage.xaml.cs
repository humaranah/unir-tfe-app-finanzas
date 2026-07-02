using HA.TFG.AppFinanzas.Core.Features.Recomendaciones;
using System.Collections.Specialized;

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
        viewModel.Messages.CollectionChanged += OnMessagesChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_resumenCargado)
            return;

        _resumenCargado = _viewModel.HasMessages;
        await _viewModel.CargarResumenAsync();
    }

    private async void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        await Task.Yield();
        await chatScrollView.ScrollToAsync(0, double.MaxValue, animated: true);
    }
}
