//using System;
//using System.Threading.Tasks;
//using System.Text;
//using S7.Net;
//using System.Collections.Generic;
//using System.Linq;
//using DebuggingTool.Database.Entity;

//namespace DebuggingTool.PLC;

//public enum StringEncoding
//{
//    ASCII,
//    UTF8,
//    Unicode,
//}

//public static class S7PlcExtension
//{
//    public static async Task BatchRead(this Plc plc, int dbNumber, List<PointBase> points)
//    {
//        if (points == null || points.Count == 0)
//            return;

//        try
//        {
//            // 确保PLC连接
//            if (!plc.IsConnected)
//                return;

//            // 计算整个DB块需要读取的范围
//            int minAddress = points.Min(p => p.StartAddress);
//            int maxAddress = points.Max(p => p.StartAddress + p.Length);
//            int totalBytes = maxAddress - minAddress;

//            // 一次性读取整个范围
//            byte[] dbData = await plc.ReadBytesAsync(
//                DataType.DataBlock,
//                dbNumber,
//                minAddress,
//                totalBytes
//            );

//            if (dbData == null || dbData.Length < totalBytes)
//            {
//                throw new Exception(
//                    $"读取数据失败，预期{totalBytes}字节，实际{dbData?.Length ?? 0}字节"
//                );
//            }

//            // 处理每个点位
//            foreach (var point in points)
//            {
//                try
//                {
//                    int offset = point.StartAddress - minAddress;
//                    object value = ExtractValueFromBytes(
//                        dbData,
//                        offset,
//                        point.Length,
//                        point.BitAddress,
//                        point.DataType,
//                        point is Point<string> stringPoint
//                            ? stringPoint.Encoding
//                            : StringEncoding.ASCII
//                    );

//                    point.SetValue(value);
//                }
//                catch (Exception ex)
//                {
//                    // 增强错误日志，包含具体位置信息
//                    Console.WriteLine(
//                        $"Error processing '{point.Name}' (Addr: {point.StartAddress}, Len: {point.Length}, Type: {point.DataType.Name}): {ex.Message}"
//                    );
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Batch read failed: {ex.Message}");
//        }
//    }

//    public static async Task BatchRead(this Plc plc,List<MonitorItem> points)
//    {
//        if (points == null || points.Count == 0)
//            return;

//        try
//        {
//            // 确保PLC连接
//            if (!plc.IsConnected)
//                return;

//            // 计算整个DB块需要读取的范围
//            int minAddress = points.Min(p => p.StartByteAdr);
//            int maxAddress = points.Max(p => p.StartByteAdr + p.Count);
//            int totalBytes = maxAddress - minAddress;

//            // 一次性读取整个范围
//            byte[] dbData = await plc.ReadBytesAsync(
//                DataType.DataBlock,
//                points[0].DB,
//                minAddress,
//                totalBytes
//            );

//            if (dbData == null || dbData.Length < totalBytes)
//            {
//                throw new Exception(
//                    $"读取数据失败，预期{totalBytes}字节，实际{dbData?.Length ?? 0}字节"
//                );
//            }

//            // 处理每个点位
//            foreach (var point in points)
//            {
//                try
//                {
//                    int offset = point.StartByteAdr - minAddress;
//                    object value = ExtractValueFromBytes(
//                        dbData,
//                        offset,
//                        point.Count,
//                        point.VarType,
//                        point.BitAdr
//                    );

//                    point.Value = value;
//                }
//                catch (Exception ex)
//                {
//                    // 增强错误日志，包含具体位置信息
//                    Console.WriteLine(
//                        $"Error processing '{point.Name}' (Addr: {point.StartByteAdr}, Len: {point.Count}, Type: {point.VarType}): {ex.Message}"
//                    );
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Batch read failed: {ex.Message}");
//        }
//    }

//    private static object ExtractValueFromBytes(
//        byte[] bytes,
//        int offset,
//        int length,
//        int bitAddress,
//        Type targetType,
//        StringEncoding encoding = StringEncoding.ASCII
//    )
//    {
//        // 验证偏移量和长度
//        if (offset < 0 || offset >= bytes.Length)
//        {
//            throw new IndexOutOfRangeException($"偏移量{offset}超出数组范围(0-{bytes.Length - 1})");
//        }

//        if (offset + length > bytes.Length)
//        {
//            throw new IndexOutOfRangeException(
//                $"数据范围越界：偏移量{offset}，长度{length}，总长度{bytes.Length}。尝试读取[{offset}-{offset + length - 1}]"
//            );
//        }

//        // 处理布尔类型
//        if (targetType == typeof(bool))
//        {
//            if (length != 1)
//                throw new ArgumentException("Bool类型长度必须为1字节");

//            if (bitAddress < 0 || bitAddress > 7)
//                throw new ArgumentException("位地址必须在0-7范围内");

//            byte mask = (byte)(1 << bitAddress);
//            return (bytes[offset] & mask) != 0;
//        }

//        // 处理字符串类型
//        if (targetType == typeof(string))
//        {
//            // 提取字符串字节数据
//            byte[] stringBytes = new byte[length];
//            Array.Copy(bytes, offset, stringBytes, 0, length);

//            // 根据编码类型转换
//            switch (encoding)
//            {
//                case StringEncoding.ASCII:
//                    return Encoding.ASCII.GetString(stringBytes).TrimEnd('\0');

//                case StringEncoding.UTF8:
//                    return Encoding.UTF8.GetString(stringBytes).TrimEnd('\0');

//                case StringEncoding.Unicode:
//                    // PLC中的Unicode通常是大端序
//                    if (BitConverter.IsLittleEndian)
//                    {
//                        for (int i = 0; i < stringBytes.Length; i += 2)
//                        {
//                            if (i + 1 < stringBytes.Length)
//                            {
//                                byte temp = stringBytes[i];
//                                stringBytes[i] = stringBytes[i + 1];
//                                stringBytes[i + 1] = temp;
//                            }
//                        }
//                    }
//                    return Encoding.Unicode.GetString(stringBytes).TrimEnd('\0');

//                default:
//                    return Encoding.ASCII.GetString(stringBytes).TrimEnd('\0');
//            }
//        }

//        // 提取所需字节
//        byte[] buffer = new byte[length];
//        Array.Copy(bytes, offset, buffer, 0, length);

//        // 根据类型确定所需的最小字节数
//        int minBytesRequired = GetMinBytesRequired(targetType);
//        if (length < minBytesRequired)
//        {
//            throw new ArgumentException(
//                $"{targetType.Name}类型需要至少{minBytesRequired}字节，但只提供了{length}字节"
//            );
//        }

//        // 处理字节序（仅多字节类型需要）
//        if (length > 1 && BitConverter.IsLittleEndian && !IsByteArrayType(targetType))
//        {
//            Array.Reverse(buffer);
//        }

//        // 根据类型转换数据
//        try
//        {
//            if (targetType == typeof(byte))
//            {
//                return buffer[0];
//            }
//            else if (targetType == typeof(sbyte))
//            {
//                return (sbyte)buffer[0];
//            }
//            else if (targetType == typeof(short))
//            {
//                return BitConverter.ToInt16(buffer, 0);
//            }
//            else if (targetType == typeof(ushort))
//            {
//                return BitConverter.ToUInt16(buffer, 0);
//            }
//            else if (targetType == typeof(int))
//            {
//                return BitConverter.ToInt32(buffer, 0);
//            }
//            else if (targetType == typeof(uint))
//            {
//                return BitConverter.ToUInt32(buffer, 0);
//            }
//            else if (targetType == typeof(long))
//            {
//                return BitConverter.ToInt64(buffer, 0);
//            }
//            else if (targetType == typeof(ulong))
//            {
//                return BitConverter.ToUInt64(buffer, 0);
//            }
//            else if (targetType == typeof(float))
//            {
//                return BitConverter.ToSingle(buffer, 0);
//            }
//            else if (targetType == typeof(double))
//            {
//                return BitConverter.ToDouble(buffer, 0);
//            }
//            else if (targetType == typeof(byte[]))
//            {
//                return buffer;
//            }
//        }
//        catch (ArgumentException ex)
//        {
//            // 添加更具体的错误信息
//            throw new ArgumentException(
//                $"{targetType.Name}类型转换失败: 缓冲区长度{length}字节，需要{minBytesRequired}字节。详细: {ex.Message}",
//                ex
//            );
//        }

//        throw new NotSupportedException($"不支持的类型: {targetType}");
//    }

//    /// <summary>
//    /// 根据VarType和长度从字节数组中提取值
//    /// </summary>
//    /// <param name="bytes">原始字节数组</param>
//    /// <param name="offset">偏移量</param>
//    /// <param name="length">要读取的字节数</param>
//    /// <param name="varType">PLC变量类型</param>
//    /// <param name="bitAddress">位地址（仅Bit类型需要）</param>
//    /// <returns>解析后的对象</returns>
//    public static object ExtractValueFromBytes(
//        byte[] bytes,
//        int offset,
//        int length,
//        VarType varType,
//        int bitAddress = 0)
//    {
//        ValidateInput(bytes, offset, length, varType);

//        try
//        {
//            switch (varType)
//            {
//                case VarType.Bit:
//                    return ExtractBit(bytes, offset, bitAddress);

//                case VarType.Byte:
//                    return ExtractByte(bytes, offset, length);

//                case VarType.Word:
//                case VarType.Int:
//                    return ExtractWord(bytes, offset, length);

//                case VarType.DWord:
//                case VarType.DInt:
//                case VarType.Real:
//                    return ExtractDWord(bytes, offset, length);

//                case VarType.LReal:
//                    return ExtractLReal(bytes, offset, length);

//                case VarType.String:
//                    return ExtractString(bytes, offset, length);

//                case VarType.Timer:
//                case VarType.Counter:
//                    return ExtractTimerOrCounter(bytes, offset, length);

//                default:
//                    throw new NotSupportedException($"不支持的VarType: {varType}");
//            }
//        }
//        catch (Exception ex)
//        {
//            throw new InvalidOperationException(
//                $"从偏移量{offset}处提取{varType}类型数据失败(长度:{length}): {ex.Message}", ex);
//        }
//    }

//    #region 具体类型提取方法（带长度验证）

//    private static bool ExtractBit(byte[] bytes, int offset, int bitAddress)
//    {
//        ValidateBitAddress(bitAddress);
//        byte mask = (byte)(1 << bitAddress);
//        return (bytes[offset] & mask) != 0;
//    }

//    private static byte ExtractByte(byte[] bytes, int offset, int length)
//    {
//        ValidateLength(length, 1, "Byte");
//        return bytes[offset];
//    }

//    private static object ExtractWord(byte[] bytes, int offset, int length)
//    {
//        ValidateLength(length, 2, "Word/Int");
//        byte[] buffer = AdjustEndian(bytes, offset, 2);
//        return buffer.Length == 2 ? BitConverter.ToInt16(buffer, 0) : (object)BitConverter.ToUInt16(buffer, 0);
//    }

//    private static object ExtractDWord(byte[] bytes, int offset, int length)
//    {
//        ValidateLength(length, 4, "DWord/DInt/Real");
//        byte[] buffer = AdjustEndian(bytes, offset, 4);

//        // 根据实际需要返回不同类型
//        return buffer.Length == 4 ? BitConverter.ToSingle(buffer, 0) : (object)BitConverter.ToInt32(buffer, 0);
//    }

//    private static double ExtractLReal(byte[] bytes, int offset, int length)
//    {
//        ValidateLength(length, 8, "LReal");
//        byte[] buffer = AdjustEndian(bytes, offset, 8);
//        return BitConverter.ToDouble(buffer, 0);
//    }

//    private static string ExtractString(byte[] bytes, int offset, int length)
//    {
//        ValidateLength(length, 3, "String");

//        byte[] buffer = new byte[length];
//        Array.Copy(bytes, offset + 2, buffer, 0, length-2);
//        return Encoding.ASCII.GetString(buffer).TrimEnd('\0');
//    }

//    private static object ExtractTimerOrCounter(byte[] bytes, int offset, int length)
//    {
//        ValidateLength(length, 2, "Timer/Counter");
//        byte[] buffer = AdjustEndian(bytes, offset, 2);
//        return BitConverter.ToUInt16(buffer, 0);
//    }

//    #endregion

//    #region 辅助方法

//    private static void ValidateInput(byte[] bytes, int offset, int length, VarType varType)
//    {
//        if (bytes == null || bytes.Length == 0)
//            throw new ArgumentNullException(nameof(bytes));

//        if (offset < 0 || offset >= bytes.Length)
//            throw new ArgumentOutOfRangeException(nameof(offset));

//        // 检查最小长度要求
//        int minLength = varType switch
//        {
//            VarType.Bit => 1,
//            VarType.Byte => 1,
//            VarType.Word or VarType.Int or VarType.Timer or VarType.Counter => 2,
//            VarType.DWord or VarType.DInt or VarType.Real => 4,
//            VarType.LReal => 8,
//            VarType.String => 1, // 字符串至少1字节
//            _ => throw new NotSupportedException($"不支持的VarType: {varType}")
//        };

//        if (length < minLength)
//            throw new ArgumentException($"{varType}类型需要至少{minLength}字节，但指定了{length}字节");

//        if (offset + length > bytes.Length)
//            throw new ArgumentOutOfRangeException(
//                $"数据范围越界：偏移量{offset}，长度{length}，总长度{bytes.Length}");
//    }

//    private static void ValidateBitAddress(int bitAddress)
//    {
//        if (bitAddress < 0 || bitAddress > 7)
//            throw new ArgumentOutOfRangeException(nameof(bitAddress), "位地址必须在0-7之间");
//    }

//    private static void ValidateLength(int actual, int required, string typeName)
//    {
//        if (actual < required)
//            throw new ArgumentException($"{typeName}需要至少{required}字节，但只提供了{actual}字节");
//    }

//    private static byte[] AdjustEndian(byte[] bytes, int offset, int length)
//    {
//        byte[] buffer = new byte[length];
//        Array.Copy(bytes, offset, buffer, 0, length);

//        // PLC使用大端序，小端系统需要反转
//        if (BitConverter.IsLittleEndian && length > 1)
//            Array.Reverse(buffer);

//        return buffer;
//    }

//    #endregion

//    private static int GetMinBytesRequired(Type targetType)
//    {
//        if (targetType == typeof(byte) || targetType == typeof(sbyte) || targetType == typeof(bool))
//            return 1;
//        if (targetType == typeof(short) || targetType == typeof(ushort))
//            return 2;
//        if (targetType == typeof(int) || targetType == typeof(uint) || targetType == typeof(float))
//            return 4;
//        if (
//            targetType == typeof(long)
//            || targetType == typeof(ulong)
//            || targetType == typeof(double)
//        )
//            return 8;
//        if (targetType == typeof(byte[]))
//            return 1; // 最小1字节
//        if (targetType == typeof(string))
//            return 1; // 最小1字节

//        return 1; // 默认
//    }

//    private static bool IsByteArrayType(Type targetType)
//    {
//        return targetType == typeof(byte[]) || targetType == typeof(string);
//    }

//    public static async Task<string> ReadString(
//        this Plc plc,
//        int dbNum,
//        int startAddress,
//        int maxStringLength = 254,
//        StringEncoding encoding = StringEncoding.ASCII
//    )
//    {
//        if (!plc.IsConnected)
//            throw new Exception($"PLC未连接，无法读取");

//        // 从 PLC 读取字节数据
//        var dataBytes = await plc.ReadBytesAsync(
//            DataType.DataBlock,
//            dbNum,
//            startAddress,
//            maxStringLength
//        );

//        // 解析实际字符串长度（第2个字节）
//        int currentLength = dataBytes[1];
//        if (currentLength > maxStringLength)
//            throw new Exception($"实际长度 {currentLength} 超过最大长度 {maxStringLength}");

//        // 提取字符数据（从第3个字节开始）
//        byte[] stringBytes = new byte[currentLength];
//        Array.Copy(dataBytes, 2, stringBytes, 0, currentLength);

//        // 根据编码类型转换
//        switch (encoding)
//        {
//            case StringEncoding.ASCII:
//                return Encoding.ASCII.GetString(stringBytes).TrimEnd('\0');

//            case StringEncoding.UTF8:
//                return Encoding.UTF8.GetString(stringBytes).TrimEnd('\0');

//            case StringEncoding.Unicode:
//                // PLC中的Unicode通常是大端序
//                if (BitConverter.IsLittleEndian)
//                {
//                    for (int i = 0; i < stringBytes.Length; i += 2)
//                    {
//                        if (i + 1 < stringBytes.Length)
//                        {
//                            byte temp = stringBytes[i];
//                            stringBytes[i] = stringBytes[i + 1];
//                            stringBytes[i + 1] = temp;
//                        }
//                    }
//                }
//                return Encoding.Unicode.GetString(stringBytes).TrimEnd('\0');

//            default:
//                return Encoding.ASCII.GetString(stringBytes).TrimEnd('\0');
//        }
//    }

//    public static async Task<T> ReadByType<T>(
//        this Plc plc,
//        int dbNum,
//        int startAddress,
//        int length,
//        int bitAddress = 0,
//        StringEncoding encoding = StringEncoding.ASCII
//    )
//        where T : struct
//    {
//        Type targetType = typeof(T);

//        // 验证长度是否足够
//        int minBytesRequired = GetMinBytesRequired(targetType);
//        if (length < minBytesRequired)
//        {
//            throw new ArgumentException(
//                $"{targetType.Name}类型需要至少{minBytesRequired}字节，但只提供了{length}字节"
//            );
//        }

//        // 确保PLC连接
//        if (!plc.IsConnected)
//            throw new Exception($"PLC未连接，无法读取");

//        // 读取原始字节数据
//        byte[] data = await plc.ReadBytesAsync(DataType.DataBlock, dbNum, startAddress, length);

//        if (data == null || data.Length < length)
//        {
//            throw new Exception($"读取数据失败，预期{length}字节，实际{data?.Length ?? 0}字节");
//        }

//        // 转换数据
//        return (T)ExtractValueFromBytes(
//            data,
//            startAddress,
//            length,
//            bitAddress,
//            targetType,
//            encoding
//        );
//    }
//}
