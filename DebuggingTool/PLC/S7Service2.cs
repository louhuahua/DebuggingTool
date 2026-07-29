using DebuggingTool.Database.Entity;
using S7.Net;
using S7.Net.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DebuggingTool.PLC;

public class S7Service2 : IDisposable
{
    public Plc? Client { get; private set; }

    private CancellationTokenSource _cts = new();
    private readonly PLCConfig _pLCConfig;
    private readonly ConcurrentDictionary<string, PointBase> _points = new();
    private bool _stopped;

    public event Action<string>? LogReceived;
    public event Action<bool>? ConnectionStatusChanged;

    public S7Service2(PLCConfig pLCConfig)
    {
        _pLCConfig = pLCConfig;
    }

    public async Task StartAync()
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
        _stopped = false;

        await Connect().ConfigureAwait(false);
        _ = RunPeriodicTimerAsync(_cts.Token);
    }

    private async Task RunPeriodicTimerAsync(CancellationToken token)
    {
        if (_pLCConfig == null)
            return;

        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(_pLCConfig.IntervalMs));

        try
        {
            while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false))
            {
                if (_stopped)
                    return;

                await 定时读取().ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            LogReceived?.Invoke("PLC定时器已取消");
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
                _pLCConfig.CpuType,
                _pLCConfig.IP,
                _pLCConfig.Port,
                _pLCConfig.Rack,
                _pLCConfig.Slot
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
            if (_stopped)
                return;

            if (Client == null || !Client.IsConnected)
            {
                await TryReconnect().ConfigureAwait(false);
                return;
            }

            if (!_points.IsEmpty)
            {
                await Client.BatchRead(_points.Values.ToList()).ConfigureAwait(false);
            }
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
        if (_stopped)
            return;

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
        if (_stopped)
            return;

        await Connect().ConfigureAwait(false);
    }

    public void Stop()
    {
        _stopped = true;

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

    public void AddPoint(PointBase point)
    {
        if (point == null)
            return;

        var key = !string.IsNullOrWhiteSpace(point.Name) ? point.Name : point.VarName;
        if (string.IsNullOrWhiteSpace(key))
        {
            key = Guid.NewGuid().ToString("N");
        }

        _points[key] = point;
    }

    public void AddPoint(List<PointBase> points)
    {
        if (points == null)
            return;

        foreach (var point in points)
        {
            AddPoint(point);
        }
    }

    public async Task<T> ReadAsync<T>(PointConfig point)
        where T : struct
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        var db = ResolveDb(point.DB);
        return await Client!
            .ReadByType<T>(db, point.StartAddress, point.Length, point.BitAddress)
            .ConfigureAwait(false);
    }

    public async Task WriteAsync<T>(PointConfig point, T t)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        var db = ResolveDb(point.DB);
        await Client!
            .WriteValue(db, point.StartAddress, point.Length, point.BitAddress, t)
            .ConfigureAwait(false);
    }

    public async Task<string> ReadStringAsync(PointConfig point)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        var db = ResolveDb(point.DB);
        return await Client!.ReadString(db, point.StartAddress, point.Length).ConfigureAwait(false);
    }

    private async Task EnsureConnectedAsync()
    {
        if (Client?.IsConnected == true)
            return;

        await TryReconnect().ConfigureAwait(false);

        if (Client?.IsConnected != true)
        {
            throw new InvalidOperationException("PLC未连接，无法执行当前操作");
        }
    }

    private int ResolveDb(int configuredDb)
    {
        return configuredDb != 0 ? configuredDb : _pLCConfig.DBNumber;
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
