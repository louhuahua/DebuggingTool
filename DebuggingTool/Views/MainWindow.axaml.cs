using Avalonia.Controls;
using DebuggingTool.Services;
using DebuggingTool.ViewModels;
using SukiUI.Controls;
using System;

namespace DebuggingTool.Views;

public partial class MainWindow : Window
{
    public static ToastService ToastService { get; private set; }
    public MainWindow()
    {
        InitializeComponent();
        ToastService = new ToastService(this);
    }
}
