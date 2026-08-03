using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using DebuggingTool.Android.Services;
using DebuggingTool.Services;
using Prism.Ioc;
using ReactiveUI.Avalonia;

namespace DebuggingTool.Android;

[Application]
public class AndroidApplication : AvaloniaAndroidApplication<App>
{
    protected AndroidApplication(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI(_ => { })
            .AfterSetup(_ =>
            {
                ContainerLocator.Current.RegisterSingleton<
                    IVibrationService,
                    AndroidVibrationService
                >();
            });
    }
}
