using System;
using AvaloniaDialogs.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DebuggingTool.Helper;
using DebuggingTool.Model;
using DebuggingTool.Services;
using Prism.Regions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace DebuggingTool.ViewModels
{
    public partial class PLCMonitorViewModel : ObservableObject
    {
        public PLCMonitorViewModel() { }

        [RelayCommand]
        private async void OpenDialog()
        {
            SingleActionDialog dialog = new()
            {
                Message = "Hello from C# code! Do you want to see a snackbar?",
                ButtonText = "Click me!",
            };
            if ((await dialog.ShowAsync()).HasValue)
            {
                MessageBus.Current.SendMessage(new SnackBarMessage("hello ava", 3));
            }
        }
    }
}
