using AvaloniaDialogs.Views;
using DebuggingTool.Database;
using DebuggingTool.Database.Entity;
using DebuggingTool.Model;
using DebuggingTool.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using S7.Net;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using static ImTools.ImMap;

namespace DebuggingTool.ViewModels
{
    public class PLCConfigViewModel : ReactiveObject
    {
        private readonly IVibrationService _vibrationService;

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

        public Array CpuTypes { get; } = Enum.GetValues(typeof(CpuType));

        public PLCConfigViewModel(IVibrationService vibrationService)
        {
            _vibrationService = vibrationService;

            LoadedCommand = ReactiveCommand.CreateFromTask(Initialize);
            ReplaceCommand = ReactiveCommand.Create(() =>
            {
                _vibrationService?.Vibrate();
                EditingConfig = new PLCConfig
                {
                    Id = SelectedConfig.Id,
                    Name = SelectedConfig.Name,
                    Ip = SelectedConfig.Ip,
                    CpuType = SelectedConfig.CpuType,
                    Port = SelectedConfig.Port,
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
                    _vibrationService?.Vibrate();
                    EditingConfig.Id = Guid.NewGuid();
                    await DB.Client.InsertAsync(EditingConfig);
                    await LoadConfigs();
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
                    _vibrationService?.Vibrate();
                    var existingConfig = await DB.Client.FindAsync<PLCConfig>(EditingConfig.Id);
                    if (existingConfig == null)
                    {
                        MessageBus.Current.SendMessage(
                            new SnackBarMessage($"无法保存修改，不存在该配置", 3)
                        );
                        return Task.CompletedTask;
                    }
                    await DB.Client.UpdateAsync(EditingConfig);
                    await LoadConfigs();
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
                    _vibrationService?.Vibrate();
                    await DB.Client.DeleteAsync(cfg);
                    // 删除满足条件的所有记录
                    await DB.Client.ExecuteAsync("DELETE FROM MonitorItem WHERE PLCConfigId = ?", cfg.Id);
                    await LoadConfigs();
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
                await LoadConfigs();
            }
            catch (Exception ex)
            {
                SingleActionDialog dialog = new()
                {
                    Message = $"初始化PLC配置界面出错：{ex.Message}",
                    ButtonText = "关闭",
                };
            }
        }

        private async Task LoadConfigs()
        {
            try
            {
                Configs = await DB.Client.Table<PLCConfig>().ToListAsync();
            }
            catch (Exception ex)
            {
                MessageBus.Current.SendMessage(
                    new SnackBarMessage($"加载PLC配置出错：{ex.Message}", 3)
                );
            }
        }
    }
}
