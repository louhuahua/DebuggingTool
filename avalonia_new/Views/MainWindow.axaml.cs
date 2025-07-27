using Avalonia.Controls;
using avalonia_new.Services;

namespace avalonia_new.Views;

public partial class MainWindow : Window
{
    public static ToastService ToastService { get; private set; }
    public MainWindow()
    {
        InitializeComponent();
        ToastService = new ToastService(this);
    }
}
