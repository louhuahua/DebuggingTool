using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using DebuggingTool.ViewModels;
using DebuggingTool.Views;
using Projektanker.Icons.Avalonia.MaterialDesign;

namespace DebuggingTool;

public partial class App : Application
{
	public override void Initialize()
	{
        IconProvider.Current
           .Register<MaterialDesignIconProvider>()
           .Register<FontAwesomeIconProvider>();

        AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow
			{
				DataContext = new MainViewModel()
			};
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
		{
			singleViewPlatform.MainView = new MainView
			{
				DataContext = new MainViewModel()
			};
		}

		base.OnFrameworkInitializationCompleted();
	}
}
