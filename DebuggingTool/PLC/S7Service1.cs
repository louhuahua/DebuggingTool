using DebuggingTool.Database.Entity;
using S7.Net;
using S7.Net.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DebuggingTool.PLC;

public class S7Service1 : IDisposable
{
    public Plc? Client { get; private set; }
    private CancellationTokenSource _cts = new();
    private PLCConfig pLCConfig;
    private bool stopped;

    public List<MonitorItem> MonitorItems { get; set; } = new();

    public event Action<string> LogReceived;
    public event Action<bool> ConnectionStatusChanged;

    public async Task Start(PLCConfig pLCConfig)
    {
        try
        {
            _cts.Cancel();
        }
        catch
        {
            // Ignore cancellation reuse errors.
        }

        _cts.Dispose();
        _cts = new CancellationTokenSource();

        this.pLCConfig = pLCConfig;
        stopped = false;

        await Connect().ConfigureAwait(false);
        _ = RunPeriodicTimerAsync(_cts.Token);
    }

    private async Task RunPeriodicTimerAsync(CancellationToken token)
    {
        if (pLCConfig == null)
            return;

        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(pLCConfig.IntervalMs));

        try
        {
            while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false))
            {
                if (stopped)
                    return;

                await 定时读取().ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            LogReceived?.Invoke("PLC定时器取消");
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
            CloseClient();

            Client = new Plc(
                pLCConfig.CpuType,
                pLCConfig.IP,
                pLCConfig.Port,
                pLCConfig.Rack,
                pLCConfig.Slot
            );

            await Client.OpenAsync().ConfigureAwait(false);

            if (Client.IsConnected)
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

            if (Client == null || !Client.IsConnected)
            {
                await TryReconnect().ConfigureAwait(false);
                return;
            }

            await Client.BatchRead(MonitorItems).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogReceived?.Invoke($"PLC读取异常: {ex.Message}");
            if (Client == null || !Client.IsConnected)
            {
                await TryReconnect().ConfigureAwait(false);
            }
        }
    }

    private async Task TryReconnect()
    {
        LogReceived?.Invoke("尝试重连PLC...");

        try
        {
            CloseClient();
        }
        catch (Exception ex)
        {
            LogReceived?.Invoke($"PLC重连异常: {ex.Message}");
        }

        await Task.Delay(3000).ConfigureAwait(false);
        if (stopped)
            return;

        await Connect().ConfigureAwait(false);
    }

    public void Stop()
    {
        stopped = true;
        try
        {
            _cts.Cancel();
        }
        catch
        {
            // Ignore cancellation reuse errors.
        }

        CloseClient();
        LogReceived?.Invoke("PLC通讯已停止");
    }

    private void CloseClient()
    {
        try
        {
            Client?.Close();
            Client = null;
        }
        catch
        {
            // Close is best-effort.
        }
    }

    public void Dispose()
    {
        Stop();
        _cts.Dispose();
    }
}
