using DebuggingTool.Database.Entity;
using DebuggingTool.PLC;
using S7.Net;
using S7.Net.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public static Task BatchRead(this Plc plc, List<PointBase> points)
    {
        return BatchReadInternal(
            plc,
            points,
            p => p.MemoryArea,
            p => p.DB,
            p => p.DB,
            p => p.StartAddress,
            p => p.Length,
            p => p.BitAddress,
            p => p.DataType,
            p => p is Point<string> stringPoint ? stringPoint.Encoding : StringEncoding.ASCII,
            (p, v) => p.SetValue(v)
        );
    }

    public static Task BatchRead(this Plc plc, List<MonitorItem> points)
    {
        return BatchReadInternal(
            plc,
            points,
            p => p.DataType,
            p => p.DB,
            p => p.DB,
            p => p.StartByteAdr,
            p => p.Count,
            p => p.BitAdr,
            p => GetTypeFromVarType(p.VarType),
            _ => StringEncoding.ASCII,
            (p, v) => p.Value = v
        );
    }

    private static async Task BatchReadInternal<T>(
        Plc plc,
        IEnumerable<T> points,
        Func<T, DataType> getMemoryArea,
        Func<T, int> getConfiguredDb,
        Func<T, int> getFallbackDb,
        Func<T, int> getStartAddress,
        Func<T, int> getByteLength,
        Func<T, int> getBitAddress,
        Func<T, Type> getDataType,
        Func<T, StringEncoding> getEncoding,
        Action<T, object> setValue
    )
    {
        if (points == null)
            return;

        var pointList = points.ToList();
        if (pointList.Count == 0)
            return;

        try
        {
            if (!plc.IsConnected)
                return;

            var readTargets = pointList
                .Select(point =>
                {
                    var memoryArea = getMemoryArea(point);
                    var db = ResolveDb(memoryArea, getConfiguredDb(point), getFallbackDb(point));
                    var startAddress = getStartAddress(point);
                    var byteLength = getByteLength(point);

                    if (byteLength <= 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(byteLength),
                            $"点位 {startAddress} 的读取长度必须大于 0"
                        );
                    }

                    return new ReadTarget<T>(
                        Point: point,
                        MemoryArea: memoryArea,
                        Db: db,
                        StartAddress: startAddress,
                        ByteLength: byteLength,
                        BitAddress: getBitAddress(point),
                        TargetType: getDataType(point),
                        Encoding: getEncoding(point)
                    );
                })
                .ToList();

            foreach (var group in readTargets.GroupBy(x => new ReadGroupKey(x.MemoryArea, x.Db)))
            {
                int minAddress = group.Min(x => x.StartAddress);
                int maxAddress = group.Max(x => x.StartAddress + x.ByteLength);
                int totalBytes = maxAddress - minAddress;

                if (totalBytes <= 0)
                    continue;

                byte[] dbData = await plc.ReadBytesAsync(
                        group.Key.MemoryArea,
                        group.Key.Db,
                        minAddress,
                        totalBytes
                    )
                    .ConfigureAwait(false);

                if (dbData == null || dbData.Length < totalBytes)
                {
                    throw new Exception(
                        $"读取 {group.Key.MemoryArea}#{group.Key.Db} 失败，预期 {totalBytes} 字节，实际 {dbData?.Length ?? 0} 字节"
                    );
                }

                foreach (var target in group)
                {
                    int offset = target.StartAddress - minAddress;
                    object value =
                        target.TargetType == typeof(string)
                            ? ExtractStringValue(dbData, offset, target.ByteLength, target.Encoding)
                            : ExtractValueFromBytes(
                                dbData,
                                offset,
                                target.ByteLength,
                                target.BitAddress,
                                target.TargetType
                            );

                    setValue(target.Point, value);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"批量读取时出错: {ex.Message}", ex);
        }
    }

    private static int ResolveDb(DataType memoryArea, int configuredDb, int fallbackDb)
    {
        if (memoryArea == DataType.DataBlock)
        {
            return configuredDb != 0 ? configuredDb : fallbackDb;
        }

        return 0;
    }

    private readonly record struct ReadGroupKey(DataType MemoryArea, int Db);

    private sealed record ReadTarget<T>(
        T Point,
        DataType MemoryArea,
        int Db,
        int StartAddress,
        int ByteLength,
        int BitAddress,
        Type TargetType,
        StringEncoding Encoding
    );

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
        if (length < 2)
            throw new ArgumentException("字符串读取长度必须至少包含 2 个字节");

        byte[] stringBytes = new byte[length];
        Array.Copy(bytes, offset, stringBytes, 0, length);

        byte[] valueBytes = new byte[length - 2];
        Array.Copy(stringBytes, 2, valueBytes, 0, length - 2);

        return encoding switch
        {
            StringEncoding.ASCII => Encoding.ASCII.GetString(valueBytes).TrimEnd('\0'),
            StringEncoding.UTF8 => Encoding.UTF8.GetString(valueBytes).TrimEnd('\0'),
            StringEncoding.Unicode => ConvertUnicodeString(valueBytes),
            _ => Encoding.ASCII.GetString(valueBytes).TrimEnd('\0'),
        };
    }

    private static string ConvertUnicodeString(byte[] bytes)
    {
        var buffer = bytes.ToArray();

        if (BitConverter.IsLittleEndian)
        {
            for (int i = 0; i < buffer.Length; i += 2)
            {
                if (i + 1 < buffer.Length)
                {
                    (buffer[i], buffer[i + 1]) = (buffer[i + 1], buffer[i]);
                }
            }
        }

        return Encoding.Unicode.GetString(buffer).TrimEnd('\0');
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
        {
            throw new ArgumentException(
                $"{targetType.Name}需要至少{minLength}字节，但指定了{length}字节"
            );
        }

        if (offset + length > bytes.Length)
        {
            throw new ArgumentOutOfRangeException(
                $"数据范围越界：偏移量{offset}，长度{length}，总长度{bytes.Length}"
            );
        }
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
            Type t when t == typeof(string) => 2,
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
            )
            .ConfigureAwait(false);
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
        int bitAddress = 0
    )
        where T : struct
    {
        if (!plc.IsConnected)
            throw new Exception("PLC未连接，无法读取");

        Type targetType = typeof(T);
        int minBytesRequired = GetMinBytesRequired(targetType);

        if (length < minBytesRequired)
        {
            throw new ArgumentException(
                $"{targetType.Name}需要至少{minBytesRequired}字节，但指定了{length}字节"
            );
        }

        byte[] data = await plc.ReadBytesAsync(DataType.DataBlock, dbNum, startAddress, length)
            .ConfigureAwait(false);

        if (data == null || data.Length < length)
            throw new Exception($"读取数据失败，预期{length}字节，实际{data?.Length ?? 0}字节");

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

        if (string.IsNullOrEmpty(writeValue))
            throw new ArgumentException("写入值不能为空");

        if (monitorItem.VarType == VarType.String)
        {
            var byteArr = Encoding.ASCII.GetBytes(writeValue);
            var len = writeValue.Length;
            var maxLen = monitorItem.Count;
            var db = ResolveDb(monitorItem.DataType, monitorItem.DB, monitorItem.DB);

            if (len > maxLen)
                throw new ArgumentException($"字符串长度 {len} 超过最大长度 {maxLen}");

            var bytes = new byte[maxLen];
            bytes[0] = (byte)len;
            bytes[1] = (byte)(maxLen - 2);

            if (len > 0)
            {
                Buffer.BlockCopy(src: byteArr, srcOffset: 0, dst: bytes, dstOffset: 2, count: len);
            }

            await plc.WriteBytesAsync(monitorItem.DataType, db, monitorItem.StartByteAdr, bytes)
                .ConfigureAwait(false);
        }
        else
        {
            object convertedValue = ConvertWriteValue(monitorItem.VarType, writeValue);
            var db = ResolveDb(monitorItem.DataType, monitorItem.DB, monitorItem.DB);

            var dataItem = new DataItem
            {
                DataType = monitorItem.DataType,
                DB = db,
                StartByteAdr = monitorItem.StartByteAdr,
                VarType = monitorItem.VarType,
                BitAdr = monitorItem.BitAdr,
                Count = monitorItem.Count,
                Value = convertedValue,
            };

            await plc.WriteAsync(new DataItem[1] { dataItem }).ConfigureAwait(false);
        }
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

    public static async Task WriteValue<T>(
        this Plc plc,
        int db,
        int startAddress,
        int length,
        byte bitAddress,
        T t
    )
    {
        if (!plc.IsConnected)
            throw new Exception("PLC未连接，无法写入");

        if (t is null)
            throw new ArgumentException("写入值不能为空");

        var varType = GetVarType<T>();

        if (varType == VarType.String)
        {
            string writeValue = t as string;
            var byteArr = Encoding.ASCII.GetBytes(writeValue);
            var len = writeValue.Length;
            var maxLen = length;

            if (len > maxLen)
                throw new ArgumentException($"字符串长度 {len} 超过最大长度 {maxLen}");

            // 创建完整缓冲区 (长度 = maxLen)，自动初始化为全0
            var bytes = new byte[maxLen];

            bytes[0] = (byte)len; // 设置实际长度
            bytes[1] = (byte)(maxLen - 2); // 设置最大长度

            // 仅当有有效数据时才复制（避免空字符串时的异常）
            if (len > 0)
            {
                Buffer.BlockCopy(
                    src: byteArr,
                    srcOffset: 0,
                    dst: bytes,
                    dstOffset: 2, // 目标数组从索引2开始
                    count: len // 只复制有效长度的字节
                );
            }

            // 注意：剩余空间已自动填充为0x00
            await plc.WriteBytesAsync(DataType.DataBlock, db, startAddress, bytes)
                .ConfigureAwait(false);
        }
        else
        {
            var dataItem = new DataItem
            {
                DataType = DataType.DataBlock,
                DB = db,
                StartByteAdr = startAddress,
                VarType = varType,
                BitAdr = bitAddress,
                Count = length,
                Value = t,
            };

            await plc.WriteAsync(new DataItem[1] { dataItem }).ConfigureAwait(false);
        }
    }

    public static VarType GetVarType<T>()
    {
        return typeof(T) switch
        {
            var t when t == typeof(bool) => VarType.Bit,
            var t when t == typeof(byte) => VarType.Byte,
            var t when t == typeof(short) => VarType.Int,
            var t when t == typeof(ushort) => VarType.Word,
            var t when t == typeof(int) => VarType.DInt,
            var t when t == typeof(uint) => VarType.DWord,
            var t when t == typeof(float) => VarType.Real,
            var t when t == typeof(double) => VarType.LReal, // 如果扩展了 LReal
            var t when t == typeof(string) => VarType.String,
            _ => throw new NotSupportedException($"不支持的类型: {typeof(T).Name}"),
        };
    }

    #endregion
}
