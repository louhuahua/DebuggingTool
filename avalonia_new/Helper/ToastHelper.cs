using avalonia_new.Views;
using static avalonia_new.Services.ToastService;

namespace avalonia_new.Helper
{
    public static class ToastHelper
    {
        public static void ShowToast(string message, ToastType type)
        {
            if (MainWindow.ToastService != null)
            {
                MainWindow.ToastService.ShowToast(message, type);
            }
            else if (MainView.ToastService != null)
            {
                MainView.ToastService.ShowToast(message, type);
            }
        }


    }
}
