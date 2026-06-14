namespace HA.TFG.AppFinanzas.App.Behaviors;

public partial class HoverBackgroundBehavior : Behavior<View>
{
    protected override void OnAttachedTo(View view)
    {
        base.OnAttachedTo(view);
        PlatformOnAttachedTo(view);
    }

    protected override void OnDetachingFrom(View view)
    {
        PlatformOnDetachingFrom(view);
        base.OnDetachingFrom(view);
    }

    partial void PlatformOnAttachedTo(View view);
    partial void PlatformOnDetachingFrom(View view);
}
