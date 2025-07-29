using System;
using S7.Net;
using SQLite;

namespace DebuggingTool.Database.Entity;

public class PLCConfig
{
    [PrimaryKey]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;

    /// <summary>
    /// PLC IP地址
    /// </summary>
    public string Ip { get; set; } = "127.0.0.1";

    /// <summary>
    /// PLC CPU类型（s7netplus用）
    /// </summary>
    public CpuType CpuType { get; set; } = CpuType.S71200;

    /// <summary>
    /// PLC机架号
    /// </summary>
    public short Rack { get; set; } = 0;

    /// <summary>
    /// PLC插槽号
    /// </summary>
    public short Slot { get; set; } = 1;

    /// <summary>
    /// 数据块
    /// </summary>
    public int DBNumber { get; set; } = 1;

    /// <summary>
    /// 执行间隔
    /// </summary>
    public int IntervalMs { get; set; } = 500;
}
