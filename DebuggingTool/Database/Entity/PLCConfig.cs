using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using S7.Net;
using SQLite;

namespace DebuggingTool.Database.Entity;

public class PLCConfig : ReactiveObject
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Reactive]
    public string Name { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;

    /// <summary>
    /// PLC IP地址
    /// </summary>
    [Reactive]
    public string Ip { get; set; } = "127.0.0.1";

    /// <summary>
    /// PLC CPU类型（s7netplus用）
    /// </summary>
    [Reactive]
    public CpuType CpuType { get; set; } = CpuType.S71200;

    /// <summary>
    /// 端口
    /// </summary>
    [Reactive]
    public int Port { get; set; } = 0;

    /// <summary>
    /// PLC机架号
    /// </summary>
    [Reactive]
    public short Rack { get; set; } = 0;

    /// <summary>
    /// PLC插槽号
    /// </summary>
    [Reactive]
    public short Slot { get; set; } = 1;

    /// <summary>
    /// 数据块
    /// </summary>
    [Reactive]
    public int DBNumber { get; set; } = 1;

    /// <summary>
    /// 执行间隔
    /// </summary>
    [Reactive]
    public int IntervalMs { get; set; } = 500;
}
