using System;

namespace DebuggingTool.PLC;

public abstract class PointBase
{
    public string Name { get; set; }
    public int StartAddress { get; set; }
    public int BitAddress { get; set; }
    public int Length { get; set; }
    public abstract Type DataType { get; }

    // 统一更新值入口
    public abstract void SetValue(object value);
}

