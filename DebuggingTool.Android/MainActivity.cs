using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Avalonia;
using Avalonia.Android;
using DebuggingTool.Android.Services;
using DebuggingTool.Services;
using Prism.Ioc;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;

namespace DebuggingTool.Android;

[Activity(
    Label = "DebuggingTool.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation
        | ConfigChanges.ScreenSize
        | ConfigChanges.UiMode
)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        {
            Window?.SetSoftInputMode(SoftInput.AdjustResize);
        }
        base.OnCreate(savedInstanceState);
        //Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
        var container = ContainerLocator.Current;
        container.RegisterSingleton<IVibrationService, AndroidVibrationService>();
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        IconProvider.Current.Register<MaterialDesignIconProvider>();

        return base.CustomizeAppBuilder(builder).WithInterFont();
    }
}
