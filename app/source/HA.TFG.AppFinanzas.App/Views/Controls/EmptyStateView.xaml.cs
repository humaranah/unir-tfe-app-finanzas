namespace HA.TFG.AppFinanzas.App.Views.Controls;

public partial class EmptyStateView : ContentView
{
    public static readonly BindableProperty TituloProperty =
        BindableProperty.Create(nameof(Titulo), typeof(string), typeof(EmptyStateView), string.Empty);

    public static readonly BindableProperty DescripcionProperty =
        BindableProperty.Create(nameof(Descripcion), typeof(string), typeof(EmptyStateView), string.Empty);

    public string Titulo
    {
        get => (string)GetValue(TituloProperty);
        set => SetValue(TituloProperty, value);
    }

    public string Descripcion
    {
        get => (string)GetValue(DescripcionProperty);
        set => SetValue(DescripcionProperty, value);
    }

    public EmptyStateView()
    {
        InitializeComponent();
    }
}
