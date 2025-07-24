using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using Station.Infrastructure.Models;

namespace Station.DAL;

public static class DB
{
    public static ISqlSugarClient Client { get; set; }

    public static void AddSqlSugarSetup(this IServiceCollection services)
    {
        Client = new SqlSugarScope(
            new ConnectionConfig()
            {
                DbType = DbType.Sqlite,
                ConnectionString = "",
                IsAutoCloseConnection = true,
            }
        );

        services.AddSingleton<ISqlSugarClient>(s => Client);
    }
}
