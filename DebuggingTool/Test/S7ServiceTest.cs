using DebuggingTool.Database.Entity;
using DebuggingTool.PLC;
using S7.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DebuggingTool.Test;

public class S7ServiceTest
{
    public static async Task TestCommunication()
    {
        var s7Service = new S7Service2(
            new PLCConfig
            {
                IP = "192.168.0.1",
                CpuType = CpuType.S71200,
                DBNumber = 1,
                IntervalMs = 100,
            }
        );
        s7Service.LogReceived += (log) =>
        {
            Debug.WriteLine(log);
        };
        s7Service.ConnectionStatusChanged += (log) =>
        {
            Debug.WriteLine($"连接：{log}");
        };

        var point1 = new PointConfig
        {
            Name = "Run",
            DB = 1,
            StartAddress = 0,
            BitAddress = 0,
            Length = 1,
        };

        var point2 = new PointConfig
        {
            Name = "Stop",
            DB = 1,
            StartAddress = 0,
            BitAddress = 1,
            Length = 1,
        };

        var point3 = new PointConfig
        {
            Name = "Bool1",
            DB = 5,
            StartAddress = 0,
            BitAddress = 0,
            Length = 1,
        };

        var point4 = new PointConfig
        {
            Name = "Str1",
            DB = 5,
            StartAddress = 2,
            BitAddress = 0,
            Length = 12,
        };

        var point5 = new PointConfig
        {
            Name = "Int1",
            DB = 5,
            StartAddress = 14,
            BitAddress = 0,
            Length = 2,
        };

        var list = new List<PointConfig> { point1, point2, point3 };

        var points = new List<PointBase>
        {
            //new Point<bool>
            //{
            //    VarName = point1.VarName,
            //    Name = point1.Name,
            //    DB = point1.DB,
            //    StartAddress = point1.StartAddress,
            //    BitAddress = point1.BitAddress,
            //    Length = point1.Length,
            //    HandleUpdate = true,
            //    UpdateAction = async (point, value) =>
            //    {
            //        Debug.WriteLine($"{point1.Name} updated to {value}");
            //        await Task.CompletedTask;
            //    },
            //    HandleChange = true,
            //    ChangedAction = async (point, value) =>
            //    {
            //        Debug.WriteLine($"=================== {point1.Name} changed to {value}");
            //        await Task.CompletedTask;
            //    },
            //},
            //new Point<bool>
            //{
            //    VarName = point2.VarName,
            //    Name = point2.Name,
            //    DB = point2.DB,
            //    StartAddress = point2.StartAddress,
            //    BitAddress = point2.BitAddress,
            //    Length = point2.Length,
            //    HandleUpdate = true,
            //    UpdateAction = async (point, value) =>
            //    {
            //        Debug.WriteLine($"{point2.Name}  updated to {value}");
            //        await Task.CompletedTask;
            //    },
            //    HandleChange = true,
            //    ChangedAction = async (point, value) =>
            //    {
            //        Debug.WriteLine($"==================={point2.Name}  changed to {value}");
            //        await Task.CompletedTask;
            //    },
            //},
            //new Point<bool>
            //{
            //    VarName = point3.VarName,
            //    Name = point3.Name,
            //    DB = point3.DB,
            //    StartAddress = point3.StartAddress,
            //    BitAddress = point3.BitAddress,
            //    Length = point3.Length,
            //    HandleUpdate = true,
            //    UpdateAction = async (point, value) =>
            //    {
            //        Debug.WriteLine($"{point3.Name} updated to {value}");
            //        await Task.CompletedTask;
            //    },
            //    HandleChange = true,
            //    ChangedAction = async (point, value) =>
            //    {
            //        Debug.WriteLine($"==================={point3.Name} changed to {value}");
            //        await Task.CompletedTask;
            //    },
            //},
            new Point<string>
            {
                VarName = point4.VarName,
                Name = point4.Name,
                DB = point4.DB,
                StartAddress = point4.StartAddress,
                BitAddress = point4.BitAddress,
                Length = point4.Length,
                HandleUpdate = true,
                UpdateAction = async (point, value) =>
                {
                    Debug.WriteLine($"{point4.Name} updated to {value}");
                    await Task.CompletedTask;
                },
                HandleChange = true,
                ChangedAction = async (point, value) =>
                {
                    Debug.WriteLine($"=================== {point4.Name} changed to {value}");
                    await Task.CompletedTask;
                },
            },
            //new Point<short>
            //{
            //    VarName = point5.VarName,
            //    Name = point5.Name,
            //    DB = point5.DB,
            //    StartAddress = point5.StartAddress,
            //    BitAddress = point5.BitAddress,
            //    Length = point5.Length,
            //    HandleUpdate = true,
            //    UpdateAction = async (point, value) =>
            //    {
            //        Debug.WriteLine($"{point5.Name} updated to {value}");
            //        await Task.CompletedTask;
            //    },
            //    HandleChange = true,
            //    ChangedAction = async (point, value) =>
            //    {
            //        Debug.WriteLine($"=================== {point5.Name} changed to {value}");
            //        await Task.CompletedTask;
            //    },
            //},
        };

        s7Service.AddPoint(points);

        await s7Service.StartAync();

        //while (true)
        //{
        //    await Task.Delay(1000);
        //}

        #region 测试读写
        //while (true)
        //{
        //    //var run = await s7Service.ReadAsync<bool>(point1);
        //    //var bool1 = await s7Service.ReadAsync<bool>(point3);
        //    //var str1 = await s7Service.ReadStringAsync(point4);
        //    //var short1 = await s7Service.ReadAsync<short>(point5);
        //    //Debug.WriteLine($"Run：{run}，boo1：{bool1}，str1：{str1}，short1：{short1}");

        //    //await Task.Delay(3000);
        //    //await s7Service.WriteAsync<bool>(point3, true);
        //    //await s7Service.WriteAsync<string>(point4, "ttt");
        //    //await s7Service.WriteAsync<short>(point5, 200);

        //    //await Task.Delay(3000);
        //    //await s7Service.WriteAsync<string>(point4, "ttt");

        //    //await Task.Delay(3000);
        //    //await s7Service.WriteAsync<string>(point4, "oooooo");

        //}
        #endregion
    }
}
