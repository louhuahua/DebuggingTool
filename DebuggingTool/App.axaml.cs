using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Preferences;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using DebuggingTool.Constants;
using DebuggingTool.Region;
using DebuggingTool.Services;
using DebuggingTool.ViewModels;
using DebuggingTool.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;
using ReactiveUI;
using System;

namespace DebuggingTool;

public class App : PrismApplication
{
    /// <summary>App entry point.</summary>
    public App()
    {
        Console.WriteLine("Constructor()");
    }

    public override void Initialize()
    {
        Console.WriteLine("Initialize()");
        AvaloniaXamlLoader.Load(this);
        base.Initialize();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        #region Services
        // Services
        containerRegistry.RegisterSingleton<Preferences>();

        #endregion

        containerRegistry.RegisterForNavigation<LoginView, LoginViewModel>();
        containerRegistry.RegisterForNavigation<LandingView, LandingViewModel>();
        containerRegistry.RegisterForNavigation<PLCMonitorView, PLCMonitorViewModel>();
        containerRegistry.RegisterForNavigation<
            HomePageContainerView,
            HomePageContainerViewModel
        >();
    }

    protected override AvaloniaObject CreateShell()
    {
        Console.WriteLine("CreateShell()");
        return ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime => Container.Resolve<MainWindow>(),
            _ => Container.Resolve<MainView>(),
        };
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
    }

    protected override void OnInitialized()
    {
        bool isDarkMode = PreferenceService.GetValue<bool>(PreferencesKeys.IS_DARK_MODE);
        if (isDarkMode)
            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
        else
            Application.Current.RequestedThemeVariant = ThemeVariant.Light;


        var regionManager = Container.Resolve<IRegionManager>();
        RegionManager.SetRegionManager(MainWindow, regionManager);
        regionManager.RegisterViewWithRegion(RegionNames.CONTENT_REGION, typeof(LandingView));
    }

    protected override void ConfigureRegionAdapterMappings(
        RegionAdapterMappings regionAdapterMappings
    )
    {
        regionAdapterMappings.RegisterMapping<ItemsControl, Region.ItemsControlRegionAdapter>();
        regionAdapterMappings.RegisterMapping<ContentControl, ContentControlRegionAdapter>();
    }
}
