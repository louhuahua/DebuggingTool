using ReactiveUI;
using ReactiveUI.SourceGenerators;
using S7.Net;
using S7.Net.Types;
using SQLite;
using System;

namespace DebuggingTool.Database.Entity;

public partial class MonitorItem : ReactiveObject
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public Guid PLCConfigId { get; set; }

    [Reactive]
    public partial string Name { get; set; } = string.Empty;

    /// <summary>
    /// Memory area to read
    /// </summary>
    [Reactive]
    public partial DataType DataType { get; set; } = DataType.DataBlock;

    /// <summary>
    /// Type of data to be read (default is bytes)
    /// </summary>
    [Reactive]
    public partial VarType VarType { get; set; } = VarType.Byte;

    /// <summary>
    /// Address of memory area to read (example: for DB1 this value is 1, for T45 this value is 45)
    /// </summary>
    [Reactive]
    public partial int DB { get; set; }

    /// <summary>
    /// Address of the first byte to read
    /// </summary>
    [Reactive]
    public partial int StartByteAdr { get; set; }

    /// <summary>
    /// Addess of bit to read from StartByteAdr
    /// </summary>
    [Reactive]
    public partial byte BitAdr { get; set; }

    /// <summary>
    /// Number of variables to read
    /// </summary>
    [Reactive]
    public partial int Count { get; set; } = 1;

    [Ignore]
    [Reactive]
    public partial object Value { get; set; }

    [Ignore]
    public string FullAddress => FormatAddress();

    private string FormatAddress()
    {
        return DataType switch
        {
            DataType.DataBlock => FormatDataBlockAddress(),
            DataType.Input => FormatAreaAddress("I", "IB", "IW", "ID"),
            DataType.Output => FormatAreaAddress("Q", "QB", "QW", "QD"),
            DataType.Memory => FormatAreaAddress("M", "MB", "MW", "MD"),
            DataType.Timer => $"T{StartByteAdr}",
            DataType.Counter => $"C{StartByteAdr}",
            _ => $"{StartByteAdr}.{BitAdr}",
        };
    }

    private string FormatDataBlockAddress()
    {
        return VarType switch
        {
            VarType.Bit => $"DB{DB}.{StartByteAdr}.{BitAdr}",
            _ => $"DB{DB}.{StartByteAdr}",
        };
    }

    private string FormatAreaAddress(string bitPrefix, string bytePrefix, string wordPrefix, string dWordPrefix)
    {
        return VarType switch
        {
            VarType.Bit => $"{bitPrefix}{StartByteAdr}.{BitAdr}",
            VarType.Byte => $"{bytePrefix}{StartByteAdr}",
            VarType.Word => $"{wordPrefix}{StartByteAdr}",
            VarType.DWord => $"{dWordPrefix}{StartByteAdr}",
            _ => $"{bytePrefix}{StartByteAdr}",
        };
    }
}
