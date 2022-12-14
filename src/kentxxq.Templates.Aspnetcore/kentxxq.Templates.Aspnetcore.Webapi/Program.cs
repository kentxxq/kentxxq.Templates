using System.Net;
using AspectCore.Extensions.DependencyInjection;
using EasyCaching.Interceptor.AspectCore;
using HealthChecks.UI.Client;
using kentxxq.Templates.Aspnetcore.Webapi.Common;
using kentxxq.Templates.Aspnetcore.Webapi.Common.Response;
using kentxxq.Templates.Aspnetcore.Webapi.Extensions;
using kentxxq.Templates.Aspnetcore.Webapi.Services;
using kentxxq.Templates.Aspnetcore.Webapi.Services.ExternalApi;
using kentxxq.Templates.Aspnetcore.Webapi.Services.Tools;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;
#if (EnableSignalR)
using kentxxq.Templates.Aspnetcore.Webapi.Hubs;
#endif
#if (EnableDB)
using kentxxq.Templates.Aspnetcore.Webapi.Services.UserInfo;
#endif
#if (EnableQuartz)
using kentxxq.Templates.Aspnetcore.Webapi.Jobs;
using Quartz;
#endif
#if (EnableNacos)
using Nacos.AspNetCore.V2;
#endif
#if (EnableRedis)
using EasyCaching.Serialization.SystemTextJson.Configurations;
#endif
#if (EnableTracing)
using Serilog.Enrichers.Span;
using System.Diagnostics;
#endif


const string appName = "kentxxq.Templates.Aspnetcore";
#if (EnableTracing)
const string logTemplate =
    "{Timestamp:HH:mm:ss}|{Level:u3}|{RequestId}|{TraceId}|{SourceContext}|{Message:lj}{Exception}{NewLine}";
#else
const string logTemplate =
    "{Timestamp:HH:mm:ss}|{Level:u3}|{RequestId}|{SourceContext}|{Message:lj}{Exception}{NewLine}";
#endif

Log.Logger = new LoggerConfiguration()
    .Filter.ByExcluding("RequestPath like '/health%'")
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Update", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient.health-checks.LogicalHandler", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient.health-checks.ClientHandler", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Infrastructure", LogEventLevel.Warning)
#if (EnableTracing)
    .Enrich.WithSpan()
#endif
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: logTemplate, theme: AnsiConsoleTheme.Code)
    .WriteTo.File(
        path: $"{ThisAssembly.Project.AssemblyName}-.log",
        formatter: new JsonFormatter(renderMessage: true),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 1)
    .CreateLogger();
Log.Information("?????????...");
Log.Information(@$"????????????: http://127.0.0.1:5000/ ??? http://127.0.0.1:5000/{appName}/ ");
Log.Information(@"swagger????????????: http://127.0.0.1:5000/swagger/index.html");
Log.Information(@"??????????????????: http://127.0.0.1:5000/healthz/startup");
Log.Information(@"??????????????????: http://127.0.0.1:5000/healthz");
Log.Information(@"????????????UI??????: http://127.0.0.1:5000/healthchecks-ui");
Log.Information(@"metrics????????????: http://127.0.0.1:5000/metrics");


try
{
    var builder = WebApplication.CreateBuilder(args);

    #region ??????????????????,??????????????????

    // ???????????????????????????
    if (!builder.Environment.IsProduction()) builder.Configuration.AddUserSecrets(typeof(Program).Assembly);
    // ????????????nginx???????????????
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.All;
    });
    // ??????????????????
    builder.Services.AddResponseCaching();
    // serilog??????
    builder.Host.UseSerilog();
    builder.Services.AddControllers();

    #region ????????????

    if (builder.Environment.IsDevelopment())
        // ????????????
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("all",
                policy =>
                {
                    policy
                        .SetIsOriginAllowed(_ => true)
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

    #endregion

    #region webapi????????????

    builder.Services.AddWebApiClient()
        .UseSourceGeneratorHttpApiActivator();
    builder.Services.AddHttpApi<IIpApi>();

    #endregion

    #region ??????aop?????????????????????

    builder.Host.UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());
    builder.Services.AddEasyCaching(option =>
    {
#if (EnableRedis)
        option.UseRedis(builder.Configuration, "redis1")
            .WithSystemTextJson("redis1");
#endif
        option.UseInMemory(builder.Configuration, "memory1");
    });
    builder.Services.ConfigureAspectCoreInterceptor(_ => { });

    #endregion

    #endregion

    builder.Services.AddMyJWT() // jwt??????
        .AddMySwagger() // swagger??????
        .AddMyRateLimiter() // ??????
        .AddMyOpentelemetry() // ??????
        .AddMyEventListener() // ???????????????
        .AddMyHealthz(builder.Configuration) // ????????????
        ;

    // ???????????????
    builder.Services.AddTransient<IDemoService, DemoService>();
    builder.Services.AddSingleton<IIpService, IpApiService>();
    builder.Services.AddSingleton<JWTService>();

    #region ??????????????????

#if (EnableDB)
    Init.InitDatabase(builder.Configuration);
    // ?????????
    builder.Services.AddSqlsugarSetup(builder.Configuration);
    builder.Services.AddTransient<IUserService, UserService>();
#endif

#if (EnableBlazor)
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
#endif

#if (EnableNacos)
    // nacos ?????????????????????
    builder.Services.AddNacosAspNet(builder.Configuration, "NacosConfig");
    // nacos ????????????
    builder.Configuration.AddNacosV2Configuration(builder.Configuration.GetSection("NacosConfig"));
    // nacos ???????????????????????????
    builder.Services.Configure<NacosSettings>(builder.Configuration.GetSection("NacosSettings"));
#endif

#if (EnableSignalR)
    //signalR
    builder.Services.AddSignalR();
#endif

#if (EnableQuartz)

    #region ????????????

    // ??????quartz?????????
    builder.Services.Configure<QuartzOptions>(builder.Configuration.GetSection("Quartz"));
    builder.Services.AddQuartz(q =>
    {
        q.UseMicrosoftDependencyInjectionJobFactory();

        q.ScheduleJob<HelloJob>(trigger =>
        {
            trigger
                .WithIdentity("hellojob", "group1")
                .StartNow()
                .WithSimpleSchedule(b =>
                {
                    b.WithIntervalInMinutes(5)
                        .RepeatForever();
                })
                .WithDescription("hellojob task");
        });


        q.ScheduleJob<DataJob>(trigger =>
        {
            trigger
                .WithIdentity("datajob", "group2")
                .UsingJobData("data", "datajob-data")
                .StartNow()
                .WithCronSchedule("5 * * * * ?")
                .WithDescription("datajob task");
        });

        //var jobKey = new JobKey("awesome job", "awesome group");
        //q.AddJob<HelloJob>(jobKey, j => j
        //    .WithDescription("my awesome job")
        //);

        //q.AddTrigger(t => t
        //    .WithIdentity("Simple Trigger")
        //    .ForJob(jobKey)
        //    .StartNow()
        //    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(10)).RepeatForever())
        //    .WithDescription("my awesome simple trigger")
        //);

        //q.AddTrigger(t => t
        //    .WithIdentity("Cron Trigger")
        //    .ForJob(jobKey)
        //    .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(3)))
        //    .WithCronSchedule("0/3 * * * * ?")
        //    .WithDescription("my awesome cron trigger")
        //);
    });
    builder.Services.AddQuartzServer(option => { option.WaitForJobsToComplete = true; });

    #endregion

#endif

    #endregion

    //var serviceList = builder.Services.ToList(); ???????????????service??????

    // ??????app??????????????????????????????
    var app = builder.Build();

    #region ???????????????????????????

    app.Lifetime.ApplicationStarted.Register(() => { Log.Information("ApplicationStarted:????????????"); });
    app.Lifetime.ApplicationStopping.Register(() =>
    {
        // shutdown?????????????????????????????????????????????
        Log.Warning("ApplicationStopping:????????????");
    });
    app.Lifetime.ApplicationStopped.Register(() => { Log.Warning("ApplicationStopped:???????????????"); });

    #endregion

    // ?????????????????????traceId
    app.Use(async (context, next) =>
    {
#if (EnableTracing)
        context.Response.Headers.Add("TraceId", Activity.Current?.TraceId.ToString());
#endif
        context.Response.Headers.Add("RequestId", context.TraceIdentifier);
        await next();
        switch (context.Response.StatusCode)
        {
            case (int)HttpStatusCode.Unauthorized:
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "application/json";
                var result = ResultModel<string>.Error("token????????????", "??????????????????????????????");
                await context.Response.WriteAsJsonAsync(result);
                break;
            }
            case (int)HttpStatusCode.Forbidden:
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.ContentType = "application/json";
                var result = ResultModel<string>.Error("????????????", "??????????????????????????????");
                await context.Response.WriteAsJsonAsync(result);
                break;
            }
        }
    });

    // ????????????
    app.UseExceptionHandler(b =>
    {
        b.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";

            var exception = context.Features.Get<IExceptionHandlerFeature>();
            if (exception != null)
            {
                var result = ResultModel<string>.Error(exception.Error.Message, exception.Error.StackTrace ?? "");
                await context.Response.WriteAsJsonAsync(result);
            }
        });
    });
    // ??????nginx????????????
    app.UseForwardedHeaders();

#if (EnableBlazor)
    app.UseWebAssemblyDebugging();
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
#endif

    // ????????? /kentxxq.Templates.Aspnetcore ??????
    // ???????????? kentxxq.com/kentxxq.Templates.Aspnetcore/api/Demo/GetData ???????????? kentxxq.com/api/Demo/GetData
    app.UsePathBase(new PathString($"/{appName}"));
    app.UseRouting();

    if (app.Environment.IsDevelopment())
    {
        app.UseCors("all");
        app.UseSwagger();
        app.UseSwaggerUI(u => { u.SwaggerEndpoint("/swagger/Examples/swagger.json", "Examples"); });
    }

    app.UseOpenTelemetryPrometheusScrapingEndpoint();

    #region ??????????????????

    app.MapHealthChecks("/healthz", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/healthz/startup", new HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("startup")
    });

    app.MapHealthChecksUI();

    #endregion

    // ??????http??????
    app.UseSerilogRequestLogging();

    // ??????????????????????????????
    app.UseAuthentication();
    app.UseAuthorization();
    // ??????????????????????????????????????????????????????????????????ratelimiter=>cache 429???????????????????????????
    app.UseRateLimiter();
    app.UseResponseCaching();

#if (EnableBlazor)
    app.MapRazorPages();
#endif

    app.MapControllers();

#if (EnableSignalR)
    app.MapHub<ChatHub>("/chatHub");
#endif


#if (EnableBlazor)
    app.MapFallbackToFile("index.html");
#endif

    app.Run();

    return 0;
}
catch (Exception exception)
{
    Log.Fatal(exception, "????????????...");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}