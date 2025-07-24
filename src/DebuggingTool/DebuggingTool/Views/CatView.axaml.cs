using Avalonia.Markup.Xaml;
using AvaloniaInside.Shell;

namespace DebuggingTool.Views;

public partial class CatView : Page
{
	public CatView()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public string Icon => "fa-solid fa-cat";
}

