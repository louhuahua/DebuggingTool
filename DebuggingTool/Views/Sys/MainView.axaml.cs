using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaDialogs.Views;
using DebuggingTool.Model;
using ReactiveUI;
using System;

namespace DebuggingTool.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        // 订阅消息
        MessageBus
            .Current.Listen<SnackBarMessage>()
            .Subscribe(msg =>
                Snackbar.Show(msg.Message, TimeSpan.FromSeconds(msg.Duration), msg.ActionText,msg.Action)
            );
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var insetsManager = TopLevel.GetTopLevel(this)?.InsetsManager;

        if (insetsManager != null)
        {
            insetsManager.DisplayEdgeToEdgePreference = true;
            insetsManager.IsSystemBarVisible = false;
        }
    }
}
