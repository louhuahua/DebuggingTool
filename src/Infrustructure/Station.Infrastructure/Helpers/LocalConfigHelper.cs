using System.Xml.Linq;
using Newtonsoft.Json;

namespace Station.Infrastructure.Helpers;

public static class LocalConfigHelper
{
    public static async Task<BaseResult<T>> ReadConfigAsync<T>()
    {
        var res = new BaseResult<T>();
        try
        {
            string path = GetFullName(typeof(T).Name);
            if (!File.Exists(path))
            {
                res.Success = false;
                res.Msg = "读取配置失败，不存在相关配置文件。";
            }
            else
            {
                string content = await File.ReadAllTextAsync(path);
                res.Data = JsonConvert.DeserializeObject<T>(content);
            }
        }
        catch (Exception ex)
        {
            res.Success = false;
            res.Msg = $"读取配置失败：{ex.Message}";
        }
        return res;
    }

    public static async Task<BaseResult> WriteConfigAsync<T>(T t)
    {
        var res = new BaseResult();
        try
        {
            string path = GetFullName(t.GetType().Name);
            var content = JsonConvert.SerializeObject(t);
            string directoryPath = Path.GetDirectoryName(path);

            // 检查并创建目录
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await File.WriteAllTextAsync(path, content);

            res.Success = true;
            res.Msg = "写入配置成功。";
        }
        catch (Exception ex)
        {
            res.Success = false;
            res.Msg = $"写入配置失败：{ex.Message}";
        }
        return res;
    }

    public static BaseResult<T> ReadConfig<T>()
    {
        var res = new BaseResult<T>();
        try
        {
            string path = GetFullName(typeof(T).Name);
            if (!File.Exists(path))
            {
                res.Success = false;
                res.Msg = "读取配置失败，不存在相关配置文件。";
            }
            else
            {
                string content = File.ReadAllText(path);
                res.Data = JsonConvert.DeserializeObject<T>(content);
            }
        }
        catch (Exception ex)
        {
            res.Success = false;
            res.Msg = $"读取配置失败：{ex.Message}";
        }
        return res;
    }

    public static BaseResult WriteConfig<T>(T t)
    {
        var res = new BaseResult();
        try
        {
            string path = GetFullName(t.GetType().Name);
            var content = JsonConvert.SerializeObject(t);
            string directoryPath = Path.GetDirectoryName(path);

            // 检查并创建目录
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(path, content);

            res.Success = true;
            res.Msg = "写入配置成功。";
        }
        catch (Exception ex)
        {
            res.Success = false;
            res.Msg = $"写入配置失败：{ex.Message}";
        }
        return res;
    }

    private static string GetFullName(string name)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory; // 获取程序根目录
        return Path.Combine(basePath, "Configs", $"{name}.json"); // 存放在 Configs 目录
    }

    public static async Task<BaseResult<T>> ReadConfigListAsync<T>()
    {
        var res = new BaseResult<T>();
        try
        {
            string path = GetListFullName(typeof(T).Name);
            if (!File.Exists(path))
            {
                res.Success = false;
                res.Msg = "读取配置失败，不存在相关配置文件。";
            }
            else
            {
                string content = await File.ReadAllTextAsync(path);
                res.List = JsonConvert.DeserializeObject<List<T>>(content);
            }
        }
        catch (Exception ex)
        {
            res.Success = false;
            res.Msg = $"读取配置失败：{ex.Message}";
        }
        return res;
    }

    public static async Task<BaseResult> WriteConfigListAsync<T>(List<T> list)
    {
        var res = new BaseResult();
        try
        {
            if (list == null || list.Count == 0)
            {
                res.Success = false;
                res.Msg = "列表为空，未写入文件。";
                return res;
            }

            string path = GetListFullName(typeof(T).Name); // 以泛型类型名命名
            string directoryPath = Path.GetDirectoryName(path);

            // 检查并创建目录
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // 序列化 List<T>
            var content = JsonConvert.SerializeObject(list, Formatting.Indented);

            await File.WriteAllTextAsync(path, content);

            res.Success = true;
            res.Msg = "写入配置成功。";
        }
        catch (Exception ex)
        {
            res.Success = false;
            res.Msg = $"写入配置失败：{ex.Message}";
        }
        return res;
    }

    public static BaseResult<T> ReadConfigList<T>()
    {
        var res = new BaseResult<T>();
        try
        {
            string path = GetListFullName(typeof(T).Name);
            if (!File.Exists(path))
            {
                res.Success = false;
                res.Msg = "读取配置失败，不存在相关配置文件。";
            }
            else
            {
                string content = File.ReadAllText(path);
                res.List = JsonConvert.DeserializeObject<List<T>>(content);
            }
        }
        catch (Exception ex)
        {
            res.Success = false;
            res.Msg = $"读取配置失败：{ex.Message}";
        }
        return res;
    }

    public static BaseResult WriteConfigList<T>(List<T> list)
    {
        var res = new BaseResult();
        try
        {
            if (list == null || list.Count == 0)
            {
                res.Success = false;
                res.Msg = "列表为空，未写入文件。";
                return res;
            }

            string path = GetListFullName(typeof(T).Name); // 以泛型类型名命名
            string directoryPath = Path.GetDirectoryName(path);

            // 检查并创建目录
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // 序列化 List<T>
            var content = JsonConvert.SerializeObject(list, Formatting.Indented);

            File.WriteAllText(path, content);

            res.Success = true;
            res.Msg = "写入配置成功。";
        }
        catch (Exception ex)
        {
            res.Success = false;
            res.Msg = $"写入配置失败：{ex.Message}";
        }
        return res;
    }

    private static string GetListFullName(string name)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory; // 获取程序根目录
        return Path.Combine(basePath, "Configs", $"{name}List.json"); // 存放在 Configs 目录
    }
}
