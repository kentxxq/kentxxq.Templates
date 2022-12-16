using kentxxq.Templates.Aspnetcore.Webapi.Common.Healthz;

namespace kentxxq.Templates.Aspnetcore.Webapi.Extensions;

/// <summary>
/// 健康检查-拓展方法
/// </summary>
public static class MyHealthzExtension
{
    /// <summary>
    /// 添加健康检查
    /// </summary>
    /// <param name="service"></param>
    /// <param name="configuration"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddMyHealthz(this IServiceCollection service,IConfiguration configuration)
    {
        
        service.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(5)
                    .DisableDatabaseMigrations()
                    .MaximumHistoryEntriesPerEndpoint(50);
            })
            .AddInMemoryStorage();
        // 有更多可用https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
        service.AddHealthChecks()
#if (EnableDB)
            // .AddMySql(configuration["Database:ConnectionString"] ?? throw new InvalidOperationException(),
            //     "k_webapi", tags: new[] { "db" })
            .AddSqlite(configuration["Database:ConnectionString"] ?? throw new InvalidOperationException(),
                "k_webapi", tags: new[] { "db" })
#endif
            .AddCheck<StartupHealthz>("startup", tags: new[] { "k8s", "startup" })
            .AddCheck<LiveHealthz>("live", tags: new[] { "k8s", "live" });

        service.AddSingleton<StartupHealthz>();
        service.AddHostedService<StartupBackgroundService>();
        return service;
    }
}