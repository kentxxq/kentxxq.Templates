
using System.Reflection;
#if (EnableDB)
using kentxxq.Templates.Aspnetcore.DB;
using Serilog;
using SqlSugar;
#endif

namespace kentxxq.Templates.Aspnetcore.Webapi.Common;

/// <summary>
/// 初始化静态工具类
/// </summary>
public static class Init
{
#if (EnableDB)
    /// <summary>
    /// 初始化数据库
    /// </summary>
    /// <param name="config"></param>
    public static void InitDatabase(IConfiguration config)
    {
        var db = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = config["Database:ConnectionString"],
            DbType = (DbType)int.Parse(config["Database:DbType"] ?? "0"),
            IsAutoCloseConnection = true
        });
        try
        {
            db.Ado.CheckConnection();
            Log.Information("数据库连接成功");
        }
        catch (Exception)
        {
            Log.Warning("数据库连接失败");
            var databaseCreated = db.DbMaintenance.CreateDatabase();
            if (databaseCreated)
            {
                Log.Warning("数据库创建成功，正在初始化中...");
#pragma warning disable CS8602 // 解引用可能出现空引用。
                var types = typeof(User).Assembly
                    .GetTypes()
                    .Where(it => it.FullName.StartsWith("kentxxq.Templates.Aspnetcore"))
                    .ToArray();
#pragma warning restore CS8602 // 解引用可能出现空引用。
                Log.Warning("开始创建表格");
                db.CodeFirst.SetStringDefaultLength(200).InitTables(types); //根据types创建表

                InitTableData(db);// 初始化数据
                return;
            }
            throw;
        }
    }
    
    private static void InitTableData(SqlSugarClient client)
    {
        var initUser = new User { Username = "ken", Password = "ken", LastLoginTime = DateTime.Now };
        var count = client.Insertable(initUser).ExecuteCommand();
        Log.Warning("初始化表数据-{UserName} {Count}条", nameof(User), count);

        var initAddresses = new List<Address>
        {
            new() { UserAddress = "qcd1", Uid = 1, Name = "ken", Phone = "123" },
            new() { UserAddress = "qcd2", Uid = 1, Name = "ken", Phone = "123" }
        };
        count = client.Insertable(initAddresses).ExecuteCommand();
        Log.Warning("初始化表数据-{AddressName} {Count}条", nameof(Address), count);
    }
#endif
}