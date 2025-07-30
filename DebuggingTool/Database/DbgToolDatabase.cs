using DebuggingTool.Constants;
using DebuggingTool.Database.Entity;
using SQLite;
using System;
using System.Threading.Tasks;

namespace DebuggingTool.Database;

public class DbgToolDatabase
{
    public SQLiteAsyncConnection Client { get; private set; }

    public async Task InitAsync()
    {
        try
        {
            if (Client is not null)
                return;

            Client = new SQLiteAsyncConnection(DBConstants.GetAppDataPath(), DBConstants.Flags);
            var count = await Client.CreateTableAsync<PLCConfig>();
            var count1 = await Client.CreateTableAsync<MonitorItem>();

            var plcConfigList = await Client.Table<PLCConfig>().ToListAsync();
            var monitorItemList = await Client.Table<MonitorItem>().ToListAsync();

            if (plcConfigList.Count == 0 && monitorItemList.Count == 0)
            {
                var plcConfig = new PLCConfig
                {
                    Id = Guid.NewGuid(),
                    Name = "Default PLC"
                };
                var result = await Client.InsertAsync(plcConfig);

                if (result > 0)
                {
                    var monitorItem = new MonitorItem
                    {
                        PLCConfigId = plcConfig.Id,
                        Name = "Default Monitor Item",
                        DataType = S7.Net.DataType.DataBlock,
                        StartByteAdr = 0,
                        BitAdr = 0,
                        Count = 1,
                        DB = plcConfig.DBNumber
                    };
                    await Client.InsertAsync(monitorItem);
                }
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        
    }
}
