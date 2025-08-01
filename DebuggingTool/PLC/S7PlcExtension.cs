using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DebuggingTool.Database.Entity;
using S7.Net;

namespace DebuggingTool.PLC;

public enum StringEncoding
{
    ASCII,
    UTF8,
    Unicode,
}

public static class S7PlcExtension
{
    #region 批量读取核心方法

    public static async Task BatchRead(this Plc plc, int dbNumber, List<PointBase> points)
    {
        await BatchReadInternal(
            plc,
            points,
            p => p.StartAddress,
            p => p.Length,
            p => p.BitAddress,
            p => p.DataType,
            p => p is Point<string> stringPoint ? stringPoint.Encoding : StringEncoding.ASCII,
            (p, v) => p.SetValue(v)
        );
    }

    public static async Task BatchRead(this Plc plc, List<MonitorItem> points)
    {
        if (points == null || points.Count == 0)
            return;

        await BatchReadInternal(
            plc,
            points,
            p => p.StartByteAdr,
            p => p.Count,
            p => p.BitAdr,
            p => GetTypeFromVarType(p.VarType),
            p => StringEncoding.ASCII,
            (p, v) => p.Value = v,
            points[0].DB
        );
    }

    private static async Task BatchReadInternal<T>(
        this Plc plc,
        IEnumerable<T> points,
        Func<T, int> getStartAddress,
        Func<T, int> getLength,
        Func<T, int> getBitAddress,
        Func<T, Type> getDataType,
        Func<T, StringEncoding> getEncoding,
        Action<T, object> setValue,
        int? fixedDbNumber = null
    )
    {
        if (points == null || !points.Any())
            return;

        try
        {
            if (!plc.IsConnected)
                return;

            // 计算读取范围
            int minAddress = points.Min(getStartAddress);
            int maxAddress = points.Max(p => getStartAddress(p) + getLength(p));
            int totalBytes = maxAddress - minAddress;

            // 确定DB块号
            int dbNumber =
                fixedDbNumber
                ?? throw new ArgumentNullException(
                    nameof(fixedDbNumber),
                    "当使用MonitorItem时需要指定DB块号"
                );

            // 批量读取
            byte[] dbData = await plc.ReadBytesAsync(
                DataType.DataBlock,
                dbNumber,
                minAddress,
                totalBytes
            );

            if (dbData == null || dbData.Length < totalBytes)
            {
                throw new Exception(
                    $"读取数据失败，预期{totalBytes}字节，实际{dbData?.Length ?? 0}字节"
                );
            }

            // 处理每个点位
            foreach (var point in points)
            {
                try
                {
                    int offset = getStartAddress(point) - minAddress;
                    int length = getLength(point);
                    object value;

                    // 根据不同类型选择提取方法
                    var dataType = getDataType(point);
                    if (dataType == typeof(string))
                    {
                        value = ExtractStringValue(dbData, offset, length, getEncoding(point));
                    }
                    else
                    {
                        value = ExtractValueFromBytes(
                            dbData,
                            offset,
                            length,
                            getBitAddress(point),
                            dataType
                        );
                    }

                    setValue(point, value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing point: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Batch read failed: {ex.Message}");
        }
    }

    #endregion

    #region 类型转换辅助方法

    private static Type GetTypeFromVarType(VarType varType)
    {
        return varType switch
        {
            VarType.Bit => typeof(bool),
            VarType.Byte => typeof(byte),
            VarType.Word => typeof(ushort),
            VarType.DWord => typeof(uint),
            VarType.Int => typeof(short),
            VarType.DInt => typeof(int),
            VarType.Real => typeof(float),
            VarType.LReal => typeof(double),
            VarType.String => typeof(string),
            VarType.Timer => typeof(ushort),
            VarType.Counter => typeof(ushort),
            _ => throw new NotSupportedException($"不支持的VarType: {varType}"),
        };
    }

    private static object ExtractStringValue(
        byte[] bytes,
        int offset,
        int length,
        StringEncoding encoding
    )
    {
        byte[] stringBytes = new byte[length];
        Array.Copy(bytes, offset, stringBytes, 0, length);

        return encoding switch
        {
            StringEncoding.ASCII => Encoding.ASCII.GetString(stringBytes).TrimEnd('\0'),
            StringEncoding.UTF8 => Encoding.UTF8.GetString(stringBytes).TrimEnd('\0'),
            StringEncoding.Unicode => ConvertUnicodeString(stringBytes),
            _ => Encoding.ASCII.GetString(stringBytes).TrimEnd('\0'),
        };
    }

    private static string ConvertUnicodeString(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian)
        {
            for (int i = 0; i < bytes.Length; i += 2)
            {
                if (i + 1 < bytes.Length)
                {
                    (bytes[i], bytes[i + 1]) = (bytes[i + 1], bytes[i]);
                }
            }
        }
        return Encoding.Unicode.GetString(bytes).TrimEnd('\0');
    }

    #endregion

    #region 核心值提取方法

    public static object ExtractValueFromBytes(
        byte[] bytes,
        int offset,
        int length,
        int bitAddress,
        Type targetType
    )
    {
        ValidateInput(bytes, offset, length, targetType);

        if (targetType == typeof(bool))
            return ExtractBit(bytes, offset, bitAddress);

        byte[] buffer = ExtractBuffer(bytes, offset, length, targetType);

        return targetType switch
        {
            Type t when t == typeof(byte) => buffer[0],
            Type t when t == typeof(sbyte) => (sbyte)buffer[0],
            Type t when t == typeof(short) => BitConverter.ToInt16(buffer, 0),
            Type t when t == typeof(ushort) => BitConverter.ToUInt16(buffer, 0),
            Type t when t == typeof(int) => BitConverter.ToInt32(buffer, 0),
            Type t when t == typeof(uint) => BitConverter.ToUInt32(buffer, 0),
            Type t when t == typeof(long) => BitConverter.ToInt64(buffer, 0),
            Type t when t == typeof(ulong) => BitConverter.ToUInt64(buffer, 0),
            Type t when t == typeof(float) => BitConverter.ToSingle(buffer, 0),
            Type t when t == typeof(double) => BitConverter.ToDouble(buffer, 0),
            Type t when t == typeof(byte[]) => buffer,
            _ => throw new NotSupportedException($"不支持的类型: {targetType}"),
        };
    }

    private static bool ExtractBit(byte[] bytes, int offset, int bitAddress)
    {
        ValidateBitAddress(bitAddress);
        byte mask = (byte)(1 << bitAddress);
        return (bytes[offset] & mask) != 0;
    }

    private static byte[] ExtractBuffer(byte[] bytes, int offset, int length, Type targetType)
    {
        byte[] buffer = new byte[length];
        Array.Copy(bytes, offset, buffer, 0, length);

        if (length > 1 && BitConverter.IsLittleEndian && !IsByteArrayType(targetType))
        {
            Array.Reverse(buffer);
        }

        return buffer;
    }

    #endregion

    #region 验证方法

    private static void ValidateInput(byte[] bytes, int offset, int length, Type targetType)
    {
        if (bytes == null || bytes.Length == 0)
            throw new ArgumentNullException(nameof(bytes));

        if (offset < 0 || offset >= bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));

        int minLength = GetMinBytesRequired(targetType);
        if (length < minLength)
            throw new ArgumentException(
                $"{targetType.Name}需要至少{minLength}字节，但指定了{length}字节"
            );

        if (offset + length > bytes.Length)
            throw new ArgumentOutOfRangeException(
                $"数据范围越界：偏移量{offset}，长度{length}，总长度{bytes.Length}"
            );
    }

    private static void ValidateBitAddress(int bitAddress)
    {
        if (bitAddress < 0 || bitAddress > 7)
            throw new ArgumentOutOfRangeException(nameof(bitAddress), "位地址必须在0-7之间");
    }

    private static int GetMinBytesRequired(Type targetType)
    {
        return targetType switch
        {
            Type t when t == typeof(bool) => 1,
            Type t when t == typeof(byte) => 1,
            Type t when t == typeof(sbyte) => 1,
            Type t when t == typeof(short) => 2,
            Type t when t == typeof(ushort) => 2,
            Type t when t == typeof(int) => 4,
            Type t when t == typeof(uint) => 4,
            Type t when t == typeof(long) => 8,
            Type t when t == typeof(ulong) => 8,
            Type t when t == typeof(float) => 4,
            Type t when t == typeof(double) => 8,
            Type t when t == typeof(byte[]) => 1,
            Type t when t == typeof(string) => 1,
            _ => 1,
        };
    }

    private static bool IsByteArrayType(Type targetType)
    {
        return targetType == typeof(byte[]) || targetType == typeof(string);
    }

    #endregion

    #region 单点读取方法

    public static async Task<string> ReadString(
        this Plc plc,
        int dbNum,
        int startAddress,
        int maxStringLength = 254,
        StringEncoding encoding = StringEncoding.ASCII
    )
    {
        if (!plc.IsConnected)
            throw new Exception("PLC未连接，无法读取");

        var dataBytes = await plc.ReadBytesAsync(
            DataType.DataBlock,
            dbNum,
            startAddress,
            maxStringLength
        );
        int currentLength = dataBytes[1];

        if (currentLength > maxStringLength)
            throw new Exception($"实际长度 {currentLength} 超过最大长度 {maxStringLength}");

        byte[] stringBytes = new byte[currentLength];
        Array.Copy(dataBytes, 2, stringBytes, 0, currentLength);

        return encoding switch
        {
            StringEncoding.ASCII => Encoding.ASCII.GetString(stringBytes).TrimEnd('\0'),
            StringEncoding.UTF8 => Encoding.UTF8.GetString(stringBytes).TrimEnd('\0'),
            StringEncoding.Unicode => ConvertUnicodeString(stringBytes),
            _ => Encoding.ASCII.GetString(stringBytes).TrimEnd('\0'),
        };
    }

    public static async Task<T> ReadByType<T>(
        this Plc plc,
        int dbNum,
        int startAddress,
        int length,
        int bitAddress = 0,
        StringEncoding encoding = StringEncoding.ASCII
    )
        where T : struct
    {
        if (!plc.IsConnected)
            throw new Exception("PLC未连接，无法读取");

        Type targetType = typeof(T);
        int minBytesRequired = GetMinBytesRequired(targetType);

        if (length < minBytesRequired)
            throw new ArgumentException(
                $"{targetType.Name}需要至少{minBytesRequired}字节，但指定了{length}字节"
            );

        byte[] data = await plc.ReadBytesAsync(DataType.DataBlock, dbNum, startAddress, length);

        if (data == null || data.Length < length)
            throw new Exception($"读取数据失败，预期{length}字节，实际{data?.Length ?? 0}字节");

        if (targetType == typeof(string))
        {
            return (T)(object)ExtractStringValue(data, 0, length, encoding);
        }

        return (T)ExtractValueFromBytes(data, 0, length, bitAddress, targetType);
    }

    #endregion

    #region PLC写入方法

    /// <summary>
    /// 根据MonitorItem配置向PLC写入字符串值
    /// </summary>
    public static async Task WriteValue(this Plc plc, MonitorItem monitorItem, string writeValue)
    {
        if (!plc.IsConnected)
            throw new Exception("PLC未连接，无法写入");

        if (string.IsNullOrWhiteSpace(writeValue))
            throw new ArgumentException("写入值不能为空");

        // 根据VarType转换WriteValue字符串为对应的数据类型
        object convertedValue = ConvertWriteValue(monitorItem.VarType, writeValue);

        // 验证转换后的值
        if (!ValidateValue(monitorItem.VarType, convertedValue))
            throw new ArgumentException("写入值格式不正确或超出范围");

        // 执行PLC写入
        await WriteToPLC(plc, monitorItem, convertedValue);
    }

    /// <summary>
    /// 根据VarType转换WriteValue字符串为对应的数据类型
    /// </summary>
    private static object ConvertWriteValue(VarType varType, string writeValue)
    {
        return varType switch
        {
            VarType.Bit => Convert.ToBoolean(writeValue),
            VarType.Byte => Convert.ToByte(writeValue),
            VarType.Word => Convert.ToUInt16(writeValue),
            VarType.DWord => Convert.ToUInt32(writeValue),
            VarType.Int => Convert.ToInt16(writeValue),
            VarType.DInt => Convert.ToInt32(writeValue),
            VarType.Real => Convert.ToSingle(writeValue),
            VarType.LReal => Convert.ToDouble(writeValue),
            VarType.String => writeValue,
            VarType.Timer => Convert.ToUInt16(writeValue),
            VarType.Counter => Convert.ToUInt16(writeValue),
            _ => throw new NotSupportedException($"不支持的VarType: {varType}"),
        };
    }

    /// <summary>
    /// 验证转换后的值是否合规
    /// </summary>
    private static bool ValidateValue(VarType varType, object value)
    {
        try
        {
            return varType switch
            {
                VarType.Bit => value is bool,
                VarType.Byte => value is byte,
                VarType.Word => value is ushort,
                VarType.DWord => value is uint,
                VarType.Int => value is short,
                VarType.DInt => value is int,
                VarType.Real => value is float,
                VarType.LReal => value is double,
                VarType.String => value is string str && str.Length <= 254, // S7字符串最大长度254
                VarType.Timer => value is ushort,
                VarType.Counter => value is ushort,
                _ => false,
            };
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 执行PLC写入操作
    /// </summary>
    private static async Task WriteToPLC(Plc plc, MonitorItem monitorItem, object value)
    {
        // 先进行显式类型转换
        object convertedValue = monitorItem.VarType switch
        {
            VarType.Byte => (byte)value,
            VarType.Word => (ushort)value,
            VarType.DWord => (uint)value,
            VarType.Int => (short)value,
            VarType.DInt => (int)value,
            VarType.Real => (float)value,
            VarType.LReal => (double)value,
            VarType.String => (string)value,
            VarType.Timer or VarType.Counter => (ushort)value,
            _ => throw new NotSupportedException($"不支持的VarType: {monitorItem.VarType}"),
        };

        // 执行PLC写入
        await plc.WriteAsync(
            monitorItem.DataType,
            monitorItem.DB,
            monitorItem.StartByteAdr,
            convertedValue
        );
    }
}

    #endregion
