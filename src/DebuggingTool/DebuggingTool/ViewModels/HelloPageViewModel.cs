using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AvaloniaInside.Shell;
using DebuggingTool.DB;
using ReactiveUI;

namespace DebuggingTool.ViewModels;

public class HelloPageViewModel : ViewModelBase
{
    private readonly INavigator _navigationService;

    public ICommand NavigateToSecondPage { get; set; }
    public ICommand ShowDialogCommand { get; set; }
    public ICommand DBTestCommand { get; set; }
    private ToolDatabase _database = new();

    public string Message { get; set; } = "greetting from viewmodel";

    public HelloPageViewModel(INavigator navigationService)
    {
        _navigationService = navigationService;
        NavigateToSecondPage = ReactiveCommand.CreateFromTask(Navigate);
        ShowDialogCommand = ReactiveCommand.CreateFromTask(ShowDialog);
        DBTestCommand = ReactiveCommand.CreateFromTask(DBTestAsync);
    }

    private Task ShowDialog(CancellationToken cancellationToken)
    {
        return _navigationService.NavigateAsync("/main/home/confirmation", cancellationToken);
    }

    private Task Navigate()
    {
        return _navigationService.NavigateAsync("/second");
    }

    public async Task DBTestAsync()
    {
        try
        {
            var count = await _database.SaveItemAsync(
            new DataItem { Name = Guid.NewGuid().ToString() }
        );

            Debug.WriteLineIf(count > 0, $"Saved {count} item(s) to the database.");

            var items = await _database.GetItemsAsync();
            foreach (var item in items)
            {
                Debug.WriteLine($"Item ID: {item.ID}, Name: {item.Name}, Done: {item.Done}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"测试读写数据库发生异常{ex.Message}");

        }

    }
}
