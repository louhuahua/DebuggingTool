using System;
using Avalonia;
using DebuggingTool.Services;
using Prism.Ioc;
using ReactiveUI.Avalonia;

namespace DebuggingTool.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI(_ => { })
            .LogToTrace()
            .AfterSetup(builder =>
            {
                var container = ContainerLocator.Current;
                container.RegisterSingleton<IVibrationService, DesktopVibrationService>();
            });
        ;
    }
}
