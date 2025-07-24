using Avalonia;
using Avalonia.Controls;

namespace DebuggingTool.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		this.AttachDevTools();
		InitializeComponent();
	}

}
