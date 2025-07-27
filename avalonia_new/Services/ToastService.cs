using Avalonia.Controls;
using Avalonia.Threading;
using avalonia_new.Controls;
using Prism.Commands;
using System;

namespace avalonia_new.Services
{
    public class ToastService
    {
        private readonly ContentControl _mainWindow;
        private ToastMessage _toastMessage;

        public enum ToastType
        {
            Success,
            Warning,
            Error
        }

        public ToastService(ContentControl mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void ShowToast(string message, ToastType type, int durationInSeconds = 3)
        {
            if (_toastMessage == null)
            {
                _toastMessage = new ToastMessage();
                var toastContainer = _mainWindow.FindControl<Panel>("ToastContainer");
                if (toastContainer != null)
                {
                    toastContainer.Children.Add(_toastMessage);
                }
            }

            _toastMessage.StatusText = type.ToString();
            _toastMessage.MessageText = message;
            _toastMessage.CloseCommand = new DelegateCommand(CloseToast);

            switch (type)
            {
                case ToastType.Success:
                    _toastMessage.TextColor = ResourceService.GetSolidColorFromKey("SuccessColor");
                    _toastMessage.FontIconText = "\uf133";
                    break;
                case ToastType.Warning:
                    _toastMessage.TextColor = ResourceService.GetSolidColorFromKey("WarningColor");
                    _toastMessage.FontIconText = "\uf028";
                    break;
                case ToastType.Error:
                    _toastMessage.TextColor = ResourceService.GetSolidColorFromKey("FailColor");
                    _toastMessage.FontIconText = "\uf159";
                    break;
            }

            _toastMessage.IsVisible = true;

            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(durationInSeconds)
            };
            timer.Tick += (sender, args) =>
            {
                _toastMessage.IsVisible = false;
                timer.Stop();
            };
            timer.Start();
        }

        private void CloseToast()
        {
            _toastMessage.IsVisible = false;
        }
    }

}
