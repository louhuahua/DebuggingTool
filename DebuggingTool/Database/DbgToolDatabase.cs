using DebuggingTool.Constants;
using DebuggingTool.Database.Entity;
using SQLite;
using System;
using System.Threading.Tasks;

namespace DebuggingTool.Database;

public class DbgToolDatabase
{
    SQLiteAsyncConnection database;

    public async Task InitAsync()
    {
        try
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(DBConstants.GetAppDataPath(), DBConstants.Flags);
            var count = await database.CreateTableAsync<PLCConfig>();
            var count1 = await database.CreateTableAsync<MonitorItem>();

            var plcConfigList = await database.Table<PLCConfig>().ToListAsync();
            var monitorItemList = await database.Table<MonitorItem>().ToListAsync();

            if (plcConfigList.Count == 0 && monitorItemList.Count == 0)
            {
                var plcConfig = new PLCConfig
                {
                    Name = "Default PLC"
                };
                var result = await database.InsertAsync(plcConfig);

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
                    await database.InsertAsync(monitorItem);
                }
            }
        }
        catch (Exception ex)
        {
            throw;
        }
        
    }
}
