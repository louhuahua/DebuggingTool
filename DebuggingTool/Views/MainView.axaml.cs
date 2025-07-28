using System;
using Avalonia.Controls;
using DebuggingTool.Model;
using DebuggingTool.Services;
using ReactiveUI;
using SukiUI.Controls;

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
                Snackbar.Show(msg.Message, TimeSpan.FromSeconds(msg.Duration), msg.ActionText)
            );
    }
}
