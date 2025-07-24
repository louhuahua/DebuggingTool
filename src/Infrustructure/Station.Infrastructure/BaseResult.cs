using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Station.Infrastructure;

public class BaseResult
{
    public bool Success { get; set; } = true;

    public string Msg { get; set; } = string.Empty;
}

public class BaseResult<T> : BaseResult
{
    /// <summary>
    /// 单个结果
    /// </summary>
    public T Data { get; set; }
    /// <summary>
    /// 多个结果
    /// </summary>
    public List<T> List { get; set; }

}
