using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DebuggingTool.Views;

public partial class MainTabControl : TabControl
{
    protected override Type StyleKeyOverride => typeof(TabControl);

    public MainTabControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

