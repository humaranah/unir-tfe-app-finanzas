using Android.App;
using Android.Content;
using Android.Content.PM;

namespace HA.TFG.AppFinanzas.App.Platforms.Android;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(
    [Intent.ActionView],
    Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
    DataScheme = CALLBACK_SCHEME)]
public class WebAuthenticatorActivity : WebAuthenticatorCallbackActivity
{
    const string CALLBACK_SCHEME = "ha.tfg.appfinanzas";
}
