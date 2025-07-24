using System.Runtime.CompilerServices;
using System.Text;

namespace Station.Infrastructure.Helpers;

public static class StringHelper
{
    private static StringBuilder sb = new StringBuilder();

    /// <summary>
    /// 日期转yyyy-MM-dd HH:mm:ss格式字符串
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static string ToCommonString(this DateTime date)
    {
        return date.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 字符串拼接，可传入多个参数
    /// </summary>
    /// <param name="str1"></param>
    /// <param name="strs"></param>
    /// <returns></returns>
    public static string AppendString(this string str1, params string[] strs)
    {
        sb.Append(str1);
        foreach (var item in strs)
        {
            sb.Append(item);
        }
        var result = sb.ToString();
        sb.Clear();
        return result;
    }
}
