using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DebuggingTool.Views.Widgets;

public partial class UserProfileWidgetView : UserControl
{
	public UserProfileWidgetView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}

