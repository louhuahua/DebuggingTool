using Avalonia.Markup.Xaml;
using AvaloniaInside.Shell;

namespace DebuggingTool.Views;

public partial class ProfileView : Page
{
	public ProfileView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}

