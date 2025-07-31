using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using S7.Net;
using S7.Net.Types;
using SQLite;
using System;

namespace DebuggingTool.Database.Entity;

public class MonitorItem:ReactiveObject
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public Guid PLCConfigId { get; set; }

    [Reactive]
    public string Name { get; set; }

    /// <summary>
    /// Memory area to read 
    /// </summary>
    public DataType DataType { get; set; }

    /// <summary>
    /// Type of data to be read (default is bytes)
    /// </summary>
    [Reactive]
    public VarType VarType { get; set; }

    /// <summary>
    /// Address of memory area to read (example: for DB1 this value is 1, for T45 this value is 45)
    /// </summary>
    [Reactive]
    public int DB { get; set; }

    /// <summary>
    /// Address of the first byte to read
    /// </summary>
    [Reactive]
    public int StartByteAdr { get; set; }

    /// <summary>
    /// Addess of bit to read from StartByteAdr
    /// </summary>
    [Reactive]
    public byte BitAdr { get; set; }

    /// <summary>
    /// Number of variables to read
    /// </summary>
    [Reactive]
    public int Count { get; set; }

    [Ignore]
    [Reactive]
    public object Value { get; set; }

    [Ignore]
    public string FullAddress => $"{StartByteAdr}.{BitAdr}";
}
