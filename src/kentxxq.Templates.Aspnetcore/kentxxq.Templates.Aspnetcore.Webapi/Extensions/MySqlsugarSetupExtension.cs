#if (EnableDB)
using SqlSugar;

namespace kentxxq.Templates.Aspnetcore.Webapi.Extensions;

/// <summary>
/// sqlsugar的DI
/// </summary>
public static class MySqlsugarSetupExtension
{
    /// <summary>
    /// 拓展方法
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static void AddSqlsugarSetup(this IServiceCollection services, IConfiguration configuration)
    {
        var sqlSugar = new SqlSugarScope(new ConnectionConfig
            {
                DbType = DbType.MySql,
                ConnectionString = configuration["Database:ConnectionString"],
                IsAutoCloseConnection = true
            },
            db =>
            {
                //单例参数配置，所有上下文生效
                db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    //Console.WriteLine(sql);//输出sql
                };
            });
        services.AddSingleton<ISqlSugarClient>(sqlSugar); //这边是SqlSugarScope用AddSingleton
    }
}
#endif