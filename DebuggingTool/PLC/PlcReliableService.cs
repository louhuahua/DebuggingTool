using DebuggingTool.Database.Entity;
using S7.Net;
using S7.Net.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DebuggingTool.PLC;

public class PLCReliableService : IDisposable
{
    private Plc _plc;
    public Plc Client { get; private set; }
    private readonly CancellationTokenSource _cts = new();
    private PLCConfig pLCConfig;
    private bool stopped;

    public List<MonitorItem> MonitorItems { get; set; }

    public event Action<string> LogReceived;
    public event Action<bool> ConnectionStatusChanged;

    public async Task Start(PLCConfig pLCConfig)
    {
        this.pLCConfig = pLCConfig;

        stopped = false;

        await Connect();

        _ = RunPeriodicTimerAsync(); // 丢弃返回的Task使其后台运行
    }

    private async Task RunPeriodicTimerAsync()
    {
        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(pLCConfig.IntervalMs));

        try
        {
            while (await timer.WaitForNextTickAsync(_cts.Token))
            {
                await 定时读取();
            }
        }
        catch (OperationCanceledException)
        {
            LogReceived?.Invoke($"PLC定时器取消");
        }
        catch (Exception ex)
        {
            LogReceived?.Invoke($"PLC读取异常: {ex.Message}");
        }
    }

    private async Task Connect()
    {
        try
        {
            _plc?.Close();
            _plc = new Plc(pLCConfig.CpuType, pLCConfig.Ip, pLCConfig.Port,pLCConfig.Rack, pLCConfig.Slot);
             await _plc.OpenAsync();

            if (_plc.IsConnected)
            {
                Client?.Close();
                Client = new Plc(pLCConfig.CpuType, pLCConfig.Ip, pLCConfig.Port, pLCConfig.Rack, pLCConfig.Slot);
                await Client.OpenAsync();
            }

            if (_plc.IsConnected)
            {
                LogReceived?.Invoke("PLC连接成功！");
                ConnectionStatusChanged?.Invoke(true);
            }
            else
            {
                LogReceived?.Invoke("PLC连接失败！");
                ConnectionStatusChanged?.Invoke(false);
            }
        }
        catch (Exception ex)
        {
            LogReceived?.Invoke($"PLC连接异常: {ex.Message}");
            ConnectionStatusChanged?.Invoke(false);
        }
    }

    private async Task 定时读取()
    {
        try
        {
            if (stopped)
                return;

            if (_plc == null || !_plc.IsConnected)
            {
                await TryReconnect();
                return;
            }

            await _plc.BatchRead(MonitorItems);

            foreach (var item in MonitorItems)
            {
                Debug.WriteLine($"读取到 {item.Name} 的值: {item.Value}");
            }
        }
        catch (Exception ex)
        {
            LogReceived?.Invoke($"PLC读取异常: {ex.Message}");
            if (_plc == null || !_plc.IsConnected)
            {
                await TryReconnect();
                return;
            }
        }
    }

    private async Task TryReconnect()
    {
        LogReceived?.Invoke("尝试重连PLC...");

        try
        {
            _plc?.Close();
            Client?.Close();
        }
        catch(Exception ex)
        {
            LogReceived?.Invoke($"PLC重连异常: {ex.Message}");
        }

        await Task.Delay(3000); // 延时3秒再重连，防止死循环重连炸死PLC
        Connect();
    }

    public void Stop()
    {
        stopped = true;
        _plc?.Close();
        Client?.Close();
        LogReceived?.Invoke("PLC通讯已停止");
    }

    public void Dispose()
    {
        Stop();
    }
}
