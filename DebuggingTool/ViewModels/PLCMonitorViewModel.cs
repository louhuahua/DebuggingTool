using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using AvaloniaDialogs.Views;
using DebuggingTool.Database;
using DebuggingTool.Database.Entity;
using DebuggingTool.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace DebuggingTool.ViewModels
{
    public class PLCMonitorViewModel : ReactiveObject
    {
        private DbgToolDatabase db;
        public ReactiveCommand<Unit, Unit> DialogCommand { get; set; }
        public ReactiveCommand<Unit, Unit> LoadedCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ReplaceCommand { get; set; }

        [Reactive]
        public PLCConfig EditingConfig { get; set; }

        [Reactive]
        public PLCConfig SelectedConfig { get; set; }

        [Reactive]
        public List<PLCConfig> Configs { get; set; }

        public PLCMonitorViewModel()
        {
            db = new DbgToolDatabase();

            //DialogCommand = ReactiveCommand.CreateFromTask(Initialize);
            LoadedCommand = ReactiveCommand.CreateFromTask(Initialize);
            ReplaceCommand = ReactiveCommand.Create(() =>
            {
                EditingConfig = SelectedConfig;
            });
        }

        private async Task Initialize()
        {
            try
            {
                await db.InitAsync();

                Configs = await db.Client.Table<PLCConfig>().ToListAsync();
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
