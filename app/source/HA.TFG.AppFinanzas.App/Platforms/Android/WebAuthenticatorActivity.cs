using Android.App;
using Android.Content;
using Android.Content.PM;

namespace HA.TFG.AppFinanzas.App.Platforms.Android;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTask, Exported = true)]
[IntentFilter(
    [Intent.ActionView],
    Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
    DataScheme = CALLBACK_SCHEME,
    DataHost = CALLBACK_HOST)]
public class WebAuthenticatorActivity : WebAuthenticatorCallbackActivity
{
    const string CALLBACK_SCHEME = "ha.tfg.appfinanzas";
    const string CALLBACK_HOST = "callback";
}
