namespace HA.TFG.AppFinanzas.App.Views.Controls;

public partial class EmptyStateView : ContentView
{
    public static readonly BindableProperty TituloProperty =
        BindableProperty.Create(nameof(Titulo), typeof(string), typeof(EmptyStateView), string.Empty);

    public static readonly BindableProperty DescripcionProperty =
        BindableProperty.Create(nameof(Descripcion), typeof(string), typeof(EmptyStateView), string.Empty);

    public string Titulo
    {
        get => GetValue(TituloProperty) as string ?? string.Empty;
        set => SetValue(TituloProperty, value);
    }

    public string Descripcion
    {
        get => GetValue(DescripcionProperty) as string ?? string.Empty;
        set => SetValue(DescripcionProperty, value);
    }

    public EmptyStateView()
    {
        InitializeComponent();
    }
}
