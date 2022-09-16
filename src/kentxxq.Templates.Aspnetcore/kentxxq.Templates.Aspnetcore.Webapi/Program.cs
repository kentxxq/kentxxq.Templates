using System.Reflection;
using kentxxq.Templates.Aspnetcore.Webapi.Services;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;
#if (EnableQuartz)
using kentxxq.Templates.Aspnetcore.Webapi.Jobs;
using Quartz;
#endif

var logTemplate = "{Timestamp:HH:mm:ss}|{Level:u3}|{SourceContext}|{Message:lj}{Exception}{NewLine}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: logTemplate, theme: AnsiConsoleTheme.Code)
    .WriteTo.File(path: $"{Assembly.GetEntryAssembly()?.GetName().Name}-.log", formatter: new JsonFormatter(),
        rollingInterval: RollingInterval.Day, retainedFileCountLimit: 1)
    .CreateLogger();
Log.Information("启动中...");


try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllers();

    // 使用serilog
    builder.Host.UseSerilog();

#if (EnableQuartz)
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
                    b.WithIntervalInSeconds(5)
                        .RepeatForever();
                })
                .WithDescription("hellojob task");
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

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(u => { u.SwaggerEndpoint("/swagger/V1/swagger.json", "V1"); });
    }

    app.MapControllers();

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