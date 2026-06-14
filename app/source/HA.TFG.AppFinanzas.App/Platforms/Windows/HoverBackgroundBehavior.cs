namespace HA.TFG.AppFinanzas.App.Behaviors;

public partial class HoverBackgroundBehavior
{
    private PointerGestureRecognizer? _pointer;
    private Color? _original;

    partial void PlatformOnAttachedTo(View view)
    {
        _pointer = new PointerGestureRecognizer();
        _pointer.PointerEntered += (_, _) =>
        {
            _original = view.BackgroundColor;
            view.BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#2D2A42") : Color.FromArgb("#D5CFF2");
        };
        _pointer.PointerExited += (_, _) => view.BackgroundColor = _original;
        view.GestureRecognizers.Add(_pointer);
    }

    partial void PlatformOnDetachingFrom(View view)
    {
        if (_pointer is not null)
            view.GestureRecognizers.Remove(_pointer);
        _pointer = null;
    }
}
