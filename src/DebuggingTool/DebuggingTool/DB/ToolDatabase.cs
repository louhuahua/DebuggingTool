using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;

namespace DebuggingTool.DB;

public class ToolDatabase
{
    SQLiteAsyncConnection database;

    async Task Init()
    {
        if (database is not null)
            return;

        database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        var result = await database.CreateTableAsync<DataGroup>();
        var result1 = await database.CreateTableAsync<DataItem>();
    }

    public async Task<List<DataItem>> GetItemsAsync()
    {
        await Init();
        return await database.Table<DataItem>().ToListAsync();
    }

    public async Task<List<DataItem>> GetItemsNotDoneAsync()
    {
        await Init();
        return await database.Table<DataItem>().Where(t => t.Done).ToListAsync();

        // SQL queries are also possible
        //return await Database.QueryAsync<DataItem>("SELECT * FROM [DataItem] WHERE [Done] = 0");
    }

    public async Task<DataItem> GetItemAsync(int id)
    {
        await Init();
        return await database.Table<DataItem>().Where(i => i.ID == id).FirstOrDefaultAsync();
    }

    public async Task<int> SaveItemAsync(DataItem item)
    {
        await Init();
        if (item.ID != 0)
            return await database.UpdateAsync(item);
        else
            return await database.InsertAsync(item);
    }

    public async Task<int> DeleteItemAsync(DataItem item)
    {
        await Init();
        return await database.DeleteAsync(item);
    }
}
