using HealthChecks.UI.Client;
using kentxxq.Templates.Aspnetcore.Webapi.Common.Healthz;
using kentxxq.Templates.Aspnetcore.Webapi.Common.Response;
using kentxxq.Templates.Aspnetcore.Webapi.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;
#if (EnableQuartz)
using kentxxq.Templates.Aspnetcore.Webapi.Jobs;
using Quartz;
#endif

var logTemplate = "{Timestamp:HH:mm:ss}|{Level:u3}|{RequestId}|{SourceContext}|{Message:lj}{Exception}{NewLine}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: logTemplate, theme: AnsiConsoleTheme.Code)
    .WriteTo.File(path: $"{ThisAssembly.Project.AssemblyName}-.log", formatter: new JsonFormatter(),
        rollingInterval: RollingInterval.Day, retainedFileCountLimit: 1)
    .CreateLogger();
Log.Information("启动中...");


try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllers();

    // serilog
    builder.Host.UseSerilog();

    // 健康检查 有更多可用https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
    builder.Services.AddHealthChecks()
        .AddCheck<StartupHealthz>("startup", tags: new[] { "k8s" })
        .AddCheck<LiveHealthz>("live", tags: new[] { "k8s" });
    builder.Services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(5)
                .DisableDatabaseMigrations()
                .MaximumHistoryEntriesPerEndpoint(50);
        })
        .AddInMemoryStorage();

    // opentelemetry

    #region opentelemetry

    builder.Services.AddOpenTelemetryMetrics(b =>
    {
        //b.AddPrometheusExporter(options =>
        //{
        //    //options.HttpListenerPrefixes = new[] { "https://localhost:443" };
        //    //options.ScrapeEndpointPath = "/api/metrics";
        //})
        //    .AddAspNetCoreInstrumentation()
        //    .AddRuntimeInstrumentation();

        b.AddPrometheusExporter()
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            ;
    });

    //builder.Services.AddOpenTelemetryTracing(x =>
    //{
    //    x.AddQuartzInstrumentation();
    //});

    #endregion

#if (EnableQuartz)

    #region quartz

    // 启用quartz定时器
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

    // 自己的服务
    builder.Services.AddSingleton<IDemoService, DemoService>();

    #region swagger

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(s =>
    {
        s.SwaggerDoc("V1", new OpenApiInfo { Title = "V1" });

        // JWT
        s.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
        {
            // http是Header带 Authorization: Bearer ZGVtbzpwQDU1dzByZA==
            // apikey 是下面3中方式
            // 参数带 /something?api_key=abcdef12345
            // header带 X-API-Key: abcdef12345
            // cookie带 Cookie: X-API-KEY=abcdef12345
            Type = SecuritySchemeType.Http,
            In = ParameterLocation.Header,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme."
        });
        s.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" }
                },
                Array.Empty<string>()
            }
        });

        // xmlDoc
        var filePath = Path.Combine(AppContext.BaseDirectory, "MyApi.xml");
        s.IncludeXmlComments(filePath);
    });

    #endregion

    var app = builder.Build();

    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("TraceId", context.TraceIdentifier);
        await next();
    });

    app.UseExceptionHandler(b =>
    {
        b.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";

            var exception = context.Features.Get<IExceptionHandlerFeature>();
            if (exception != null)
            {
                var result = ResultModel<string>.Error(exception.Error.StackTrace ?? "", exception.Error.Message);
                await context.Response.WriteAsJsonAsync(result);
            }
        });
    });

    // 移除掉 /kentxxq.Templates.Aspnetcore 前缀
    // 例如请求 kentxxq.com/kentxxq.Templates.Aspnetcore/api/Demo/GetData 就会变成 kentxxq.com/api/Demo/GetData
    app.UsePathBase(new PathString("/kentxxq.Templates.Aspnetcore"));
    app.UseRouting();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(u => { u.SwaggerEndpoint("/swagger/V1/swagger.json", "V1"); });
    }

    app.UseOpenTelemetryPrometheusScrapingEndpoint();

    //app.MapHealthChecks("/healthz", new HealthCheckOptions
    //{
    //    AllowCachingResponses = false
    //});

    app.MapControllers();

    app.UseEndpoints(config =>
    {
        config.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        // healthchecks-ui
        config.MapHealthChecksUI();
    });

    app.Run();

    return 0;
}
catch (Exception exception)
{
    Log.Fatal(exception, "异常退出...");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}