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
using S7.Net;

namespace DebuggingTool.ViewModels
{
    public class PLCMonitorViewModel : ReactiveObject
    {
        private DbgToolDatabase db;
        public ReactiveCommand<Unit, Unit> DialogCommand { get; set; }
        public ReactiveCommand<Unit, Unit> LoadedCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ReplaceCommand { get; set; }
        public ReactiveCommand<Unit, Task> AddCommand { get; set; }
        public ReactiveCommand<Unit, Task> EditCommand { get; set; }
        public ReactiveCommand<PLCConfig, Task> RemoveCommand { get; set; }

        [Reactive]
        public PLCConfig EditingConfig { get; set; }

        [Reactive]
        public PLCConfig SelectedConfig { get; set; }

        [Reactive]
        public List<PLCConfig> Configs { get; set; }

        [Reactive]
        public Array CpuTypes { get; } = Enum.GetValues(typeof(CpuType));

        public PLCMonitorViewModel()
        {
            db = new DbgToolDatabase();

            //DialogCommand = ReactiveCommand.CreateFromTask(Initialize);
            LoadedCommand = ReactiveCommand.CreateFromTask(Initialize);
            ReplaceCommand = ReactiveCommand.Create(() =>
            {
                EditingConfig = new PLCConfig
                {
                    Id = SelectedConfig.Id,
                    Name = SelectedConfig.Name,
                    Ip = SelectedConfig.Ip,
                    CpuType = SelectedConfig.CpuType,
                    Rack = SelectedConfig.Rack,
                    Slot = SelectedConfig.Slot,
                    DBNumber = SelectedConfig.DBNumber,
                    IntervalMs = SelectedConfig.IntervalMs,
                    CreateDate = SelectedConfig.CreateDate,
                };
            });
            AddCommand = ReactiveCommand.CreateFromTask<Unit, Task>(async _ =>
            {
                try
                {
                    EditingConfig.Id = Guid.NewGuid();
                    await db.Client.InsertAsync(EditingConfig);
                    Configs = await db.Client.Table<PLCConfig>().ToListAsync();
                }
                catch (Exception ex)
                {
                    MessageBus.Current.SendMessage(
                        new SnackBarMessage($"添加PLC配置出错：{ex.Message}", 3)
                    );
                }
                return Task.CompletedTask;
            });
            EditCommand = ReactiveCommand.CreateFromTask<Unit, Task>(async _ =>
            {
                try
                {
                    var existingConfig = await db.Client.FindAsync<PLCConfig>(EditingConfig.Id);
                    if (existingConfig == null)
                    {
                        MessageBus.Current.SendMessage(
                            new SnackBarMessage($"无法保存修改，不存在该配置", 3)
                        );
                        return Task.CompletedTask;
                    }
                    await db.Client.UpdateAsync(EditingConfig);
                    Configs = await db.Client.Table<PLCConfig>().ToListAsync();
                }
                catch (Exception ex)
                {
                    MessageBus.Current.SendMessage(
                        new SnackBarMessage($"修改PLC配置出错：{ex.Message}", 3)
                    );
                }
                return Task.CompletedTask;
            });

            RemoveCommand = ReactiveCommand.CreateFromTask<PLCConfig, Task>(async cfg =>
            {
                try
                {
                    await db.Client.DeleteAsync(cfg);
                    Configs = await db.Client.Table<PLCConfig>().ToListAsync();
                }
                catch (Exception ex)
                {
                    MessageBus.Current.SendMessage(
                        new SnackBarMessage($"删除PLC配置出错：{ex.Message}", 3)
                    );
                }
                return Task.CompletedTask;
            });
        }

        private async Task Initialize()
        {
            try
            {
                EditingConfig = new PLCConfig { Id = default };
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
