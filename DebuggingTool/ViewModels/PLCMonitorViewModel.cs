using AvaloniaDialogs.Views;
using DebuggingTool.Database;
using DebuggingTool.Model;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace DebuggingTool.ViewModels
{
    public  class PLCMonitorViewModel : ReactiveObject
    {
        private DbgToolDatabase database;
        public ReactiveCommand<Unit, Unit> DialogCommand { get; set; }


        public PLCMonitorViewModel()
        {
            DialogCommand = ReactiveCommand.CreateFromTask(OpenDialog);
            database = new DbgToolDatabase();
        }

        private async Task OpenDialog()
        {
            try
            {
                await database.InitAsync();
            }
            catch (Exception ex)
            {
                SingleActionDialog dialog = new()
                {
                    Message = $"初始化数据库出错：{ex.Message}",
                    ButtonText = "关闭",
                };
            }
            //SingleActionDialog dialog = new()
            //{
            //    Message = "Hello from C# code! Do you want to see a snackbar?",
            //    ButtonText = "Click me!",
            //};
            //if ((await dialog.ShowAsync()).HasValue)
            //{
            //    MessageBus.Current.SendMessage(new SnackBarMessage("hello ava", 3));
            //}
        }
    }
}
