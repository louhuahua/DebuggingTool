using DebuggingTool.Database;
using ReactiveUI;

namespace DebuggingTool.ViewModels;

public partial class MainViewModel : ReactiveObject
{
    public MainViewModel()
    {
            _ = DB.InitAsync();
    }
}
