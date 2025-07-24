using System.Text;
using S7.Net;

namespace Station.Hardware.PLC;

public enum StringEncoding
{
    ASCII,
    UTF8,
    Unicode,
}

public static class S7PlcExtension
{
    public static async Task BatchRead(this Plc plc, int dbNumber, List<PointBase> points)
    {
        if (points == null || points.Count == 0)
            return;

        try
        {
            // 确保PLC连接
            if (!plc.IsConnected)
                return;

            // 计算整个DB块需要读取的范围
            int minAddress = points.Min(p => p.StartAddress);
            int maxAddress = points.Max(p => p.StartAddress + p.Length);
            int totalBytes = maxAddress - minAddress;

            // 一次性读取整个范围
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
                    int offset = point.StartAddress - minAddress;
                    object value = ExtractValueFromBytes(
                        dbData,
                        offset,
                        point.Length,
                        point.BitAddress,
                        point.DataType,
                        point is Point<string> stringPoint
                            ? stringPoint.Encoding
                            : StringEncoding.ASCII
                    );

                    point.SetValue(value);
                }
                catch (Exception ex)
                {
                    // 增强错误日志，包含具体位置信息
                    Console.WriteLine(
                        $"Error processing '{point.Name}' (Addr: {point.StartAddress}, Len: {point.Length}, Type: {point.DataType.Name}): {ex.Message}"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Batch read failed: {ex.Message}");
        }
    }

    private static object ExtractValueFromBytes(
        byte[] bytes,
        int offset,
        int length,
        int bitAddress,
        Type targetType,
        StringEncoding encoding = StringEncoding.ASCII
    )
    {
        // 验证偏移量和长度
        if (offset < 0 || offset >= bytes.Length)
        {
            throw new IndexOutOfRangeException($"偏移量{offset}超出数组范围(0-{bytes.Length - 1})");
        }

        if (offset + length > bytes.Length)
        {
            throw new IndexOutOfRangeException(
                $"数据范围越界：偏移量{offset}，长度{length}，总长度{bytes.Length}。尝试读取[{offset}-{offset + length - 1}]"
            );
        }

        // 处理布尔类型
        if (targetType == typeof(bool))
        {
            if (length != 1)
                throw new ArgumentException("Bool类型长度必须为1字节");

            if (bitAddress < 0 || bitAddress > 7)
                throw new ArgumentException("位地址必须在0-7范围内");

            byte mask = (byte)(1 << bitAddress);
            return (bytes[offset] & mask) != 0;
        }

        // 处理字符串类型
        if (targetType == typeof(string))
        {
            // 提取字符串字节数据
            byte[] stringBytes = new byte[length];
            Array.Copy(bytes, offset, stringBytes, 0, length);

            // 根据编码类型转换
            switch (encoding)
            {
                case StringEncoding.ASCII:
                    return Encoding.ASCII.GetString(stringBytes).TrimEnd('\0');

                case StringEncoding.UTF8:
                    return Encoding.UTF8.GetString(stringBytes).TrimEnd('\0');

                case StringEncoding.Unicode:
                    // PLC中的Unicode通常是大端序
                    if (BitConverter.IsLittleEndian)
                    {
                        for (int i = 0; i < stringBytes.Length; i += 2)
                        {
                            if (i + 1 < stringBytes.Length)
                            {
                                byte temp = stringBytes[i];
                                stringBytes[i] = stringBytes[i + 1];
                                stringBytes[i + 1] = temp;
                            }
                        }
                    }
                    return Encoding.Unicode.GetString(stringBytes).TrimEnd('\0');

                default:
                    return Encoding.ASCII.GetString(stringBytes).TrimEnd('\0');
            }
        }

        // 提取所需字节
        byte[] buffer = new byte[length];
        Array.Copy(bytes, offset, buffer, 0, length);

        // 根据类型确定所需的最小字节数
        int minBytesRequired = GetMinBytesRequired(targetType);
        if (length < minBytesRequired)
        {
            throw new ArgumentException(
                $"{targetType.Name}类型需要至少{minBytesRequired}字节，但只提供了{length}字节"
            );
        }

        // 处理字节序（仅多字节类型需要）
        if (length > 1 && BitConverter.IsLittleEndian && !IsByteArrayType(targetType))
        {
            Array.Reverse(buffer);
        }

        // 根据类型转换数据
        try
        {
            if (targetType == typeof(byte))
            {
                return buffer[0];
            }
            else if (targetType == typeof(sbyte))
            {
                return (sbyte)buffer[0];
            }
            else if (targetType == typeof(short))
            {
                return BitConverter.ToInt16(buffer, 0);
            }
            else if (targetType == typeof(ushort))
            {
                return BitConverter.ToUInt16(buffer, 0);
            }
            else if (targetType == typeof(int))
            {
                return BitConverter.ToInt32(buffer, 0);
            }
            else if (targetType == typeof(uint))
            {
                return BitConverter.ToUInt32(buffer, 0);
            }
            else if (targetType == typeof(long))
            {
                return BitConverter.ToInt64(buffer, 0);
            }
            else if (targetType == typeof(ulong))
            {
                return BitConverter.ToUInt64(buffer, 0);
            }
            else if (targetType == typeof(float))
            {
                return BitConverter.ToSingle(buffer, 0);
            }
            else if (targetType == typeof(double))
            {
                return BitConverter.ToDouble(buffer, 0);
            }
            else if (targetType == typeof(byte[]))
            {
                return buffer;
            }
        }
        catch (ArgumentException ex)
        {
            // 添加更具体的错误信息
            throw new ArgumentException(
                $"{targetType.Name}类型转换失败: 缓冲区长度{length}字节，需要{minBytesRequired}字节。详细: {ex.Message}",
                ex
            );
        }

        throw new NotSupportedException($"不支持的类型: {targetType}");
    }

    private static int GetMinBytesRequired(Type targetType)
    {
        if (targetType == typeof(byte) || targetType == typeof(sbyte) || targetType == typeof(bool))
            return 1;
        if (targetType == typeof(short) || targetType == typeof(ushort))
            return 2;
        if (targetType == typeof(int) || targetType == typeof(uint) || targetType == typeof(float))
            return 4;
        if (
            targetType == typeof(long)
            || targetType == typeof(ulong)
            || targetType == typeof(double)
        )
            return 8;
        if (targetType == typeof(byte[]))
            return 1; // 最小1字节
        if (targetType == typeof(string))
            return 1; // 最小1字节

        return 1; // 默认
    }

    private static bool IsByteArrayType(Type targetType)
    {
        return targetType == typeof(byte[]) || targetType == typeof(string);
    }

    public static async Task<string> ReadString(
        this Plc plc,
        int dbNum,
        int startAddress,
        int maxStringLength = 254,
        StringEncoding encoding = StringEncoding.ASCII
    )
    {
        if (!plc.IsConnected)
            throw new Exception($"PLC未连接，无法读取");

        // 从 PLC 读取字节数据
        var dataBytes = await plc.ReadBytesAsync(
            DataType.DataBlock,
            dbNum,
            startAddress,
            maxStringLength
        );

        // 解析实际字符串长度（第2个字节）
        int currentLength = dataBytes[1];
        if (currentLength > maxStringLength)
            throw new Exception($"实际长度 {currentLength} 超过最大长度 {maxStringLength}");

        // 提取字符数据（从第3个字节开始）
        byte[] stringBytes = new byte[currentLength];
        Array.Copy(dataBytes, 2, stringBytes, 0, currentLength);

        // 根据编码类型转换
        switch (encoding)
        {
            case StringEncoding.ASCII:
                return Encoding.ASCII.GetString(stringBytes).TrimEnd('\0');

            case StringEncoding.UTF8:
                return Encoding.UTF8.GetString(stringBytes).TrimEnd('\0');

            case StringEncoding.Unicode:
                // PLC中的Unicode通常是大端序
                if (BitConverter.IsLittleEndian)
                {
                    for (int i = 0; i < stringBytes.Length; i += 2)
                    {
                        if (i + 1 < stringBytes.Length)
                        {
                            byte temp = stringBytes[i];
                            stringBytes[i] = stringBytes[i + 1];
                            stringBytes[i + 1] = temp;
                        }
                    }
                }
                return Encoding.Unicode.GetString(stringBytes).TrimEnd('\0');

            default:
                return Encoding.ASCII.GetString(stringBytes).TrimEnd('\0');
        }
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
        Type targetType = typeof(T);

        // 验证长度是否足够
        int minBytesRequired = GetMinBytesRequired(targetType);
        if (length < minBytesRequired)
        {
            throw new ArgumentException(
                $"{targetType.Name}类型需要至少{minBytesRequired}字节，但只提供了{length}字节"
            );
        }

        // 确保PLC连接
        if (!plc.IsConnected)
            throw new Exception($"PLC未连接，无法读取");

        // 读取原始字节数据
        byte[] data = await plc.ReadBytesAsync(DataType.DataBlock, dbNum, startAddress, length);

        if (data == null || data.Length < length)
        {
            throw new Exception($"读取数据失败，预期{length}字节，实际{data?.Length ?? 0}字节");
        }

        // 转换数据
        return (T)ExtractValueFromBytes(
            data,
            startAddress,
            length,
            bitAddress,
            targetType,
            encoding
        );
    }
}
