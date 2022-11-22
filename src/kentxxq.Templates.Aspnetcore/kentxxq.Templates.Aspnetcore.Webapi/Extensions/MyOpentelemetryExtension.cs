using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;

namespace kentxxq.Templates.Aspnetcore.Webapi.Extensions;

/// <summary>
/// opentelemetry-拓展方法
/// </summary>
public static class MyOpentelemetryExtension
{
    private const string AppName = "kentxxq.Templates.Aspnetcore";

    /// <summary>
    /// 添加opentelemetry
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddMyOpentelemetry(this IServiceCollection service)
    {
        AddMetrics(service);
        return service;
    }

    /// <summary>
    /// 添加 metrics 指标数据
    /// </summary>
    /// <param name="service"></param>
    private static void AddMetrics(IServiceCollection service)
    {
        var meter = new Meter(AppName,"1.0.0");
        service.AddSingleton(meter);
        
        service.AddOpenTelemetryMetrics(b =>
        {
            //b.AddPrometheusExporter(options =>
            //{
            //    //options.HttpListenerPrefixes = new[] { "https://localhost:443" };
            //    //options.ScrapeEndpointPath = "/api/metrics";
            //})
            //    .AddAspNetCoreInstrumentation()
            //    .AddRuntimeInstrumentation();

            b.AddPrometheusExporter()
                .AddMeter(AppName)
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddEventCountersInstrumentation(o =>
                {
                    o.RefreshIntervalSecs = 1;
                    // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/available-counters
                    // o.AddEventSources("System.Runtime"); 有RuntimeInstrumentation和AspNetCoreInstrumentation就够了
                    o.AddEventSources("Microsoft.AspNetCore.Hosting");
                    // o.AddEventSources("Microsoft.AspNetCore.Http.Connections"); 没看到输出
                    o.AddEventSources("Microsoft-AspNetCore-Server-Kestrel");
                    o.AddEventSources("System.Net.Http");
                    o.AddEventSources("System.Net.NameResolution");
                    // o.AddEventSources("System.Net.Security"); 主要是ssl信息，挂在代理后面不需要这个
                    o.AddEventSources("System.Net.Sockets"); 
                })
                ;
        });
    }

    /// <summary>
    /// 添加 trace 追踪数据
    /// </summary>
    /// <param name="service"></param>
    private static void AddTrace(IServiceCollection service)
    {
        // TODO
        //builder.Services.AddOpenTelemetryTracing(x =>
        //{
        //    x.AddQuartzInstrumentation();
        //});
    }
}