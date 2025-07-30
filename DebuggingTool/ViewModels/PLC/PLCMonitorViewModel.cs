using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AvaloniaDialogs.Views;
using DebuggingTool.Database;
using DebuggingTool.Database.Entity;
using DebuggingTool.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using S7.Net;

namespace DebuggingTool.ViewModels
{
    public class PLCMonitorViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> LoadedCommand { get; set; }

        [Reactive]
        public List<PLCConfig> Configs { get; set; }

        [Reactive]
        public PLCConfig SelectedConfig { get; set; }

        [Reactive]
        public List<MonitorItem> MonitorItems { get; set; }

        [Reactive]
        public MonitorItem SelectedMonitorItem { get; set; }

        public PLCMonitorViewModel()
        {
            LoadedCommand = ReactiveCommand.CreateFromTask(Initialize);

            this.WhenAnyValue(x => x.SelectedConfig)
                .Where(config => config != null)
                .SelectMany(config =>
                    DB.Client.Table<MonitorItem>()
                        .Where(m => m.PLCConfigId == config.Id)
                        .ToListAsync()
                )
                .Subscribe(items => MonitorItems = items);
        }

        private async Task Initialize()
        {
            try
            {
                Configs = await DB.Client.Table<PLCConfig>().ToListAsync();
                SelectedConfig = Configs.Count > 0 ? Configs[0] : null;
            }
            catch (Exception ex)
            {
                SingleActionDialog dialog = new()
                {
                    Message = $"初始化PLC监控界面出错：{ex.Message}",
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
