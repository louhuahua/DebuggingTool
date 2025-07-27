using Avalonia.Controls;
using avalonia_new.Services;

namespace avalonia_new.Views;

public partial class MainView : UserControl
{
    public static ToastService ToastService { get; private set; }
    public MainView()
    {
        InitializeComponent();
        ToastService = new ToastService(this);
    }
}
