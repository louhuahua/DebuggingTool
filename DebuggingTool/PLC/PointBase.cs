using S7.Net;
using System;
using System.ComponentModel.DataAnnotations;
using S7DataType = S7.Net.DataType;

namespace DebuggingTool.PLC;

public abstract class PointBase
{
    public string Name { get; set; } = string.Empty;

    public string VarName { get; set; } = string.Empty;

    public S7DataType MemoryArea { get; set; } = S7DataType.DataBlock;

    public int DB { get; set; }

    public int StartAddress { get; set; }

    public int BitAddress { get; set; }

    public int Length { get; set; }

    public abstract Type DataType { get; }

    /// <summary>
    /// 仅opcua订阅模式
    /// </summary>
    public bool OnlySubscribeMode { get; set; }

    public bool HandleUpdate { get; set; }

    public bool HandleChange { get; set; }

    // 统一更新值入口
    public abstract void SetValue(object value);
}

