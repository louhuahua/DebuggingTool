using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AvaloniaDialogs.Views;
using DebuggingTool.Database;
using DebuggingTool.Database.Entity;
using DebuggingTool.Model;
using DebuggingTool.PLC;
using DebuggingTool.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using S7.Net;

namespace DebuggingTool.ViewModels
{
    public class PLCMonitorViewModel : ReactiveObject
    {
        private readonly IVibrationService _vibrationService;
        private PLCReliableService pLCReliableService;
        private bool initialized;

        public ReactiveCommand<Unit, Unit> LoadedCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ExpandCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ReplaceCommand { get; set; }
        public ReactiveCommand<Unit, Task> AddCommand { get; set; }
        public ReactiveCommand<Unit, Task> EditCommand { get; set; }
        public ReactiveCommand<Unit, Task> RemoveCommand { get; set; }
        public ReactiveCommand<Unit, Task> WriteCommand { get; set; }
        public ReactiveCommand<bool, Task> WriteBitCommand { get; set; }

        [Reactive]
        public List<PLCConfig> Configs { get; set; }

        [Reactive]
        public PLCConfig SelectedConfig { get; set; }

        [Reactive]
        public List<MonitorItem> MonitorItems { get; set; }

        [Reactive]
        public MonitorItem EditingMonitorItem { get; set; }

        [Reactive]
        public MonitorItem SelectedMonitorItem { get; set; }

        [Reactive]
        public bool Expanded { get; set; }

        [Reactive]
        public bool Monitoring { get; set; }

        public Array VarTypes { get; } = Enum.GetValues(typeof(VarType));

        [Reactive]
        public bool SendLog { get; set; }

        [Reactive]
        public string WriteValue { get; set; }

        public PLCMonitorViewModel(IVibrationService vibrationService)
        {
            _vibrationService = vibrationService;

            #region 监控属性
            this.WhenAnyValue(x => x.Expanded)
                .Subscribe(item =>
                {
                    if (item)
                    {
                        Monitoring = false;
                    }
                });

            this.WhenAnyValue(x => x.Monitoring)
                .Subscribe(async item =>
                {
                    _vibrationService?.Vibrate();
                    if (item)
                    {
                        await pLCReliableService?.Start(SelectedConfig);
                        MessageBus.Current.SendMessage(new SnackBarMessage("PLC监控已启动", 1));
                    }
                    else
                    {
                        pLCReliableService?.Stop();
                        MessageBus.Current.SendMessage(new SnackBarMessage("PLC监控已停止", 1));
                    }
                });

            this.WhenAnyValue(x => x.SelectedConfig)
                .Where(config => config != null)
                .SelectMany(config =>
                    DB.Client.Table<MonitorItem>()
                        .Where(m => m.PLCConfigId == config.Id)
                        .ToListAsync()
                )
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(items =>
                {
                    Monitoring = false;
                    MonitorItems = items;
                });

            this.WhenAnyValue(x => x.MonitorItems)
                .Where(items => items != null)
                .Subscribe(items =>
                {
                    pLCReliableService.MonitorItems = items;
                });
            #endregion

            #region 初始化命令
            LoadedCommand = ReactiveCommand.CreateFromTask(Initialize);
            ExpandCommand = ReactiveCommand.Create(() =>
            {
                Expanded = !Expanded;
                _vibrationService?.Vibrate();
            });
            ReplaceCommand = ReactiveCommand.Create(() =>
            {
                _vibrationService?.Vibrate();
                Expanded = true;
                EditingMonitorItem = new MonitorItem
                {
                    Id = SelectedMonitorItem.Id,
                    PLCConfigId = SelectedMonitorItem.PLCConfigId,
                    Name = SelectedMonitorItem.Name,
                    DataType = DataType.DataBlock,
                    VarType = SelectedMonitorItem.VarType,
                    DB = SelectedMonitorItem.DB,
                    StartByteAdr = SelectedMonitorItem.StartByteAdr,
                    BitAdr = SelectedMonitorItem.BitAdr,
                    Count = SelectedMonitorItem.Count,
                };
            });
            AddCommand = ReactiveCommand.CreateFromTask<Unit, Task>(async _ =>
            {
                try
                {
                    _vibrationService?.Vibrate();
                    EditingMonitorItem.Id = Guid.NewGuid();
                    EditingMonitorItem.PLCConfigId = SelectedConfig.Id;
                    EditingMonitorItem.DB = SelectedConfig.DBNumber;
                    await DB.Client.InsertAsync(EditingMonitorItem);
                    await LoadMonitorItems();
                }
                catch (Exception ex)
                {
                    MessageBus.Current.SendMessage(
                        new SnackBarMessage($"添加监控项出错：{ex.Message}", 3)
                    );
                }
                return Task.CompletedTask;
            });
            EditCommand = ReactiveCommand.CreateFromTask<Unit, Task>(async _ =>
            {
                try
                {
                    _vibrationService?.Vibrate();
                    var existingItem = await DB.Client.FindAsync<MonitorItem>(
                        EditingMonitorItem.Id
                    );
                    if (existingItem == null)
                    {
                        MessageBus.Current.SendMessage(
                            new SnackBarMessage($"无法保存修改，不存在该项", 3)
                        );
                        return Task.CompletedTask;
                    }
                    await DB.Client.UpdateAsync(EditingMonitorItem);
                    await LoadMonitorItems();
                }
                catch (Exception ex)
                {
                    MessageBus.Current.SendMessage(
                        new SnackBarMessage($"修改PLC配置出错：{ex.Message}", 3)
                    );
                }
                return Task.CompletedTask;
            });

            RemoveCommand = ReactiveCommand.CreateFromTask<Unit, Task>(async _ =>
            {
                try
                {
                    if (SelectedMonitorItem == null)
                    {
                        MessageBus.Current.SendMessage(new SnackBarMessage($"请先选择任意一项", 3));
                        return Task.CompletedTask;
                    }

                    _vibrationService?.Vibrate();
                    await DB.Client.DeleteAsync(SelectedMonitorItem);
                    await LoadMonitorItems();
                }
                catch (Exception ex)
                {
                    MessageBus.Current.SendMessage(
                        new SnackBarMessage($"删除PLC配置出错：{ex.Message}", 3)
                    );
                }
                return Task.CompletedTask;
            });

            WriteCommand = ReactiveCommand.CreateFromTask<Unit, Task>(async _ =>
            {
                try
                {
                    _vibrationService?.Vibrate();

                    if (SelectedMonitorItem == null)
                    {
                        MessageBus.Current.SendMessage(
                            new SnackBarMessage($"请先选择要写入的监控项", 3)
                        );
                        return Task.CompletedTask;
                    }

                    if (string.IsNullOrEmpty(WriteValue))
                    {
                        MessageBus.Current.SendMessage(new SnackBarMessage($"请输入要写入的值", 3));
                        return Task.CompletedTask;
                    }

                    if (
                        pLCReliableService?.Client == null
                        || !pLCReliableService.Client.IsConnected
                    )
                    {
                        MessageBus.Current.SendMessage(
                            new SnackBarMessage($"PLC未连接，无法写入", 3)
                        );
                        return Task.CompletedTask;
                    }

                    // 使用扩展方法执行PLC写入
                    await pLCReliableService.Client.WriteValue(SelectedMonitorItem, WriteValue);

                    MessageBus.Current.SendMessage(
                        new SnackBarMessage($"成功写入值: {WriteValue}", 1)
                    );
                    //WriteValue = string.Empty; // 清空写入值
                }
                catch (Exception ex)
                {
                    MessageBus.Current.SendMessage(
                        new SnackBarMessage($"PLC写入出错：{ex.Message}", 3)
                    );
                }
                return Task.CompletedTask;
            });

            WriteBitCommand = ReactiveCommand.CreateFromTask<bool, Task>(async bitValue =>
            {
                try
                {
                    _vibrationService?.Vibrate();

                    if (SelectedMonitorItem == null)
                    {
                        MessageBus.Current.SendMessage(
                            new SnackBarMessage($"请先选择要写入的监控项", 3)
                        );
                        return Task.CompletedTask;
                    }

                    if (SelectedMonitorItem.VarType != VarType.Bit)
                    {
                        MessageBus.Current.SendMessage(
                            new SnackBarMessage($"只能向Bit类型的监控项写入布尔值", 3)
                        );
                        return Task.CompletedTask;
                    }

                    if (
                        pLCReliableService?.Client == null
                        || !pLCReliableService.Client.IsConnected
                    )
                    {
                        MessageBus.Current.SendMessage(
                            new SnackBarMessage($"PLC未连接，无法写入", 3)
                        );
                        return Task.CompletedTask;
                    }

                    // 直接写入Bit值
                    await pLCReliableService.Client.WriteBitAsync(
                        SelectedMonitorItem.DataType,
                        SelectedMonitorItem.DB,
                        SelectedMonitorItem.StartByteAdr,
                        SelectedMonitorItem.BitAdr,
                        bitValue
                    );

                    MessageBus.Current.SendMessage(
                        new SnackBarMessage($"成功写入Bit值: {bitValue}", 1)
                    );
                }
                catch (Exception ex)
                {
                    MessageBus.Current.SendMessage(
                        new SnackBarMessage($"PLC Bit写入出错：{ex.Message}", 3)
                    );
                }
                return Task.CompletedTask;
            });
            #endregion
        }

        private async Task Initialize()
        {
            try
            {
                Configs = await DB.Client.Table<PLCConfig>().ToListAsync();

                EditingMonitorItem = new MonitorItem
                {
                    Id = default,
                    DataType = DataType.DataBlock,
                };
                SelectedConfig = Configs.Count > 0 ? Configs[0] : null;

                if (!initialized)
                {
                    pLCReliableService = new PLCReliableService();
                    pLCReliableService.LogReceived -= OnLogRecived;
                    pLCReliableService.LogReceived += OnLogRecived;
                    pLCReliableService.ConnectionStatusChanged -= OnConnectionStatusChanged;
                    pLCReliableService.ConnectionStatusChanged += OnConnectionStatusChanged;
                    initialized = true;
                }
            }
            catch (Exception ex)
            {
                SingleActionDialog dialog = new()
                {
                    Message = $"初始化PLC监控界面出错：{ex.Message}",
                    ButtonText = "关闭",
                };
            }
        }

        private async Task LoadMonitorItems()
        {
            try
            {
                MonitorItems = await DB
                    .Client.Table<MonitorItem>()
                    .Where(i => i.PLCConfigId == SelectedConfig.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(
                    new SnackBarMessage($"加载监控项配置出错：{ex.Message}", 3)
                );
            }
        }

        private void OnLogRecived(string msg)
        {
            if (!SendLog)
                return;
            MessageBus.Current.SendMessage(new SnackBarMessage($"PLC服务：{msg}", 1));
        }

        private void OnConnectionStatusChanged(bool status)
        {
            if (!SendLog)
                return;
            MessageBus.Current.SendMessage(new SnackBarMessage($"PLC连接：{status}", 1));
        }
    }
}
