using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
#if (EnableTracing)
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
#endif
namespace kentxxq.Templates.Aspnetcore.Webapi.Extensions;

/// <summary>
/// opentelemetry-拓展方法
/// </summary>
public static class MyOpentelemetryExtension
{
    private const string AppName = "kentxxq.Templates.Aspnetcore";
    private const string AppVersion = "1.0.0";

    /// <summary>
    /// 添加opentelemetry
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddMyOpentelemetry(this IServiceCollection service)
    {
        AddMetrics(service);
#if (EnableTracing)
        AddTrace(service);
#endif
        return service;
    }

    /// <summary>
    /// 添加 metrics 指标数据
    /// </summary>
    /// <param name="service"></param>
    private static void AddMetrics(IServiceCollection service)
    {
        service.AddOpenTelemetry()
            .ConfigureResource(builder => builder.AddService(AppName, serviceVersion: AppVersion))
            .WithMetrics(builder => builder.AddPrometheusExporter()
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
            );
    }
#if (EnableTracing)
    /// <summary>
    /// 添加 trace 追踪数据
    /// </summary>
    /// <param name="service"></param>
    private static void AddTrace(IServiceCollection service)
    {
        service.AddOpenTelemetryTracing(x =>
        {
            x.AddSource(AppName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(AppName,AppVersion))
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.Filter = r => r.Request.Path != "/healthz";
                })
                .AddHttpClientInstrumentation(o =>
                {
                    o.FilterHttpRequestMessage = r => r.RequestUri?.PathAndQuery != "/healthz";
                })
                .AddQuartzInstrumentation()
                // .AddConsoleExporter()
                // .AddOtlpExporter(o =>
                // {
                //     o.Endpoint = new Uri("http://8.142.70.33:4317");
                //     o.Protocol = OtlpExportProtocol.Grpc;
                // });
                
                // docker run -d --name zipkin -p 9411:9411 openzipkin/zipkin 
                // .AddZipkinExporter(o =>
                // {
                //     o.Endpoint = new Uri("http://8.142.70.33:9411/api/v2/spans");
                // });
                
                // docker run -d --name jaeger \
                // -e COLLECTOR_ZIPKIN_HOST_PORT=:9411 \
                // -e COLLECTOR_OTLP_ENABLED=true \
                // -e JAEGER_SAMPLER_TYPE=const \
                // -e JAEGER_SAMPLER_PARAM=1 \
                // -e JAEGER_REPORTER_LOG_SPANS=true \
                // -p 6831:6831/udp \
                // -p 6832:6832/udp \
                // -p 5778:5778 \
                // -p 16686:16686 \
                // -p 4317:4317 \
                // -p 4318:4318 \
                // -p 14250:14250 \
                // -p 14268:14268 \
                // -p 14269:14269 \
                // -p 9411:9411 \
                // jaegertracing/all-in-one:1.39
                .AddJaegerExporter(o =>
                {
                    o.AgentHost = "8.142.70.33";
                    o.AgentPort = 6831;
                });
        });
    }
#endif
}