namespace HA.TFG.AppFinanzas.App.Views.Controls;

public partial class DetalleFilaView : ContentView
{
    public static readonly BindableProperty EtiquetaProperty =
        BindableProperty.Create(nameof(Etiqueta), typeof(string), typeof(DetalleFilaView), string.Empty);

    public static readonly BindableProperty ValorProperty =
        BindableProperty.Create(nameof(Valor), typeof(string), typeof(DetalleFilaView), string.Empty);

    public string Etiqueta
    {
        get => (string)GetValue(EtiquetaProperty);
        set => SetValue(EtiquetaProperty, value);
    }

    public string Valor
    {
        get => (string)GetValue(ValorProperty);
        set => SetValue(ValorProperty, value);
    }

    public DetalleFilaView()
    {
        InitializeComponent();
    }
}
