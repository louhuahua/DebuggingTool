using S7.Net;

public class PLCConfig
{
    /// <summary>
    /// PLC IP地址
    /// </summary>
    public string Ip { get; set; }

    /// <summary>
    /// PLC CPU类型（s7netplus用）
    /// </summary>
    public CpuType CpuType { get; set; }


    /// <summary>
    /// PLC机架号
    /// </summary>
    public short Rack { get; set; }

    /// <summary>
    /// PLC插槽号
    /// </summary>
    public short Slot { get; set; }

    /// <summary>
    /// 数据块
    /// </summary>
    public int DBNumber { get; set; }


    /// <summary>
    /// 执行间隔
    /// </summary>
    public int IntervalMs { get; set; }
}
