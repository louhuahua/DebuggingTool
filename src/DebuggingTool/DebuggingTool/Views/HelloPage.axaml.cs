using Avalonia.Markup.Xaml;
using AvaloniaInside.Shell;
using System.Threading;
using System.Threading.Tasks;

namespace DebuggingTool.Views;

public partial class HelloPage : Page
{
	public HelloPage()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

    public override Task InitialiseAsync(CancellationToken cancellationToken)
	{
		DataContext = new ViewModels.HelloPageViewModel(Navigator);
		return Task.CompletedTask;
	}
}
