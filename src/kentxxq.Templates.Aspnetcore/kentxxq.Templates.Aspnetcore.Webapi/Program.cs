using AspectCore.Extensions.DependencyInjection;
using EasyCaching.Interceptor.AspectCore;
using HealthChecks.UI.Client;
using kentxxq.Templates.Aspnetcore.Webapi.Common.Healthz;
using kentxxq.Templates.Aspnetcore.Webapi.Common.Response;
using kentxxq.Templates.Aspnetcore.Webapi.Services;
using kentxxq.Templates.Aspnetcore.Webapi.Services.ExternalApi;
using kentxxq.Templates.Aspnetcore.Webapi.Services.Tools;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;
#if (EnableSignalR)
using kentxxq.Templates.Aspnetcore.Webapi.Hubs;
#endif
#if (EnableDB)
using kentxxq.Templates.Aspnetcore.Webapi.Extensions;
using kentxxq.Templates.Aspnetcore.Webapi.Services.UserInfo;
#endif
#if (EnableQuartz)
using kentxxq.Templates.Aspnetcore.Webapi.Jobs;
using Quartz;
#endif
#if (EnableNacos)
using Nacos.AspNetCore.V2;
using kentxxq.Templates.Aspnetcore.Webapi.Common;
#endif
#if (EnableRedis)
using EasyCaching.Serialization.SystemTextJson.Configurations;
#endif
using System.Net;

var logTemplate = "{Timestamp:HH:mm:ss}|{Level:u3}|{RequestId}|{SourceContext}|{Message:lj}{Exception}{NewLine}";

Log.Logger = new LoggerConfiguration()
    .Filter.ByExcluding("RequestPath like '/health%'")
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Update", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient.health-checks.LogicalHandler", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient.health-checks.ClientHandler", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Infrastructure", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: logTemplate, theme: AnsiConsoleTheme.Code)
    .WriteTo.File(path: $"{ThisAssembly.Project.AssemblyName}-.log", formatter: new JsonFormatter(renderMessage: true),
        rollingInterval: RollingInterval.Day, retainedFileCountLimit: 1)
    .CreateLogger();
Log.Information("启动中...");
Log.Information(@"请求地址: http://127.0.0.1:5000/ 或 http://127.0.0.1:5000/kentxxq.Templates.Aspnetcore/ ");
Log.Information(@"swagger请求地址: http://127.0.0.1:5000/swagger/index.html");
Log.Information(@"就绪检查地址: http://127.0.0.1:5000/healthz/startup");
Log.Information(@"存活检查地址: http://127.0.0.1:5000/healthz");
Log.Information(@"健康检查UI地址: http://127.0.0.1:5000/healthchecks-ui");


try
{
    var builder = WebApplication.CreateBuilder(args);
    if (!builder.Environment.IsProduction())
    {
        builder.Configuration.AddUserSecrets(typeof(Program).Assembly);
    }
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;
    });

    #region JWT配置
    
    builder.Services.AddAuthentication("Bearer") // controller没有配置方案的时候，使用Bearer认证
        .AddJwtBearer() // 等同于AddJwtBearer("Bearer")
        .AddJwtBearer("Test");

    builder.Services.AddAuthorization(options =>
    {
        // 角色不能满足，或多种条件组合的时候。采用自定义
        // dotnet user-jwts create  --claim is_allow=true --claim ken=ken_allow --role admin --role superadmin --audience dotnet-user-jwts  
        options.AddPolicy("is_allow", policy => {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("is_allow", "true");
            // 多种claim条件组合
            policy.RequireAssertion((context =>
            {
                return context.User.HasClaim(c => c.Type == "ken" && (c.Value == "ken_allow" || c.Value=="admin_allow") );
            }));
        });
    });

    #endregion

    #region 限制配置

    builder.Services.AddMyRateLimiter();

    #endregion

#if (EnableBlazor)
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();
#endif

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

#if (EnableNacos)
    // nacos 服务注册与发现
    builder.Services.AddNacosAspNet(builder.Configuration, "NacosConfig");
    // nacos 配置中心
    builder.Configuration.AddNacosV2Configuration(builder.Configuration.GetSection("NacosConfig"));
    // nacos 读取对应配置到对象
    builder.Services.Configure<NacosSettings>(builder.Configuration.GetSection("NacosSettings"));
#endif

    builder.Services.AddControllers();

    if (builder.Environment.IsDevelopment())
        // 跨域配置
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

#if (EnableSignalR)
    //signalR
    builder.Services.AddSignalR();
#endif

    // 启用响应缓存
    builder.Services.AddResponseCaching();
    // serilog日志
    builder.Host.UseSerilog();

#if (EnableDB)
    // 数据库
    builder.Services.AddSqlsugarSetup(builder.Configuration);
#endif

    #region 健康检查

    builder.Services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(5)
                .DisableDatabaseMigrations()
                .MaximumHistoryEntriesPerEndpoint(50);
        })
        .AddInMemoryStorage();
    // 有更多可用https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
    builder.Services.AddHealthChecks()
#if (EnableDB)
        .AddMySql(builder.Configuration["Database:ConnectionString"] ?? throw new InvalidOperationException(), "k_webapi", tags: new[] { "db" })
#endif
        .AddCheck<StartupHealthz>("startup", tags: new[] { "k8s", "startup" })
        .AddCheck<LiveHealthz>("live", tags: new[] { "k8s", "live" });

    builder.Services.AddSingleton<StartupHealthz>();
    builder.Services.AddHostedService<StartupBackgroundService>();

    #endregion

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
    builder.Services.AddTransient<IDemoService, DemoService>();
    builder.Services.AddSingleton<IIpService, IpApiService>();
    builder.Services.AddSingleton<JWTService>();
#if (EnableDB)
    builder.Services.AddTransient<IUserService, UserService>();
#endif

    #region webapi自动生成

    builder.Services.AddWebApiClient()
        .UseSourceGeneratorHttpApiActivator();
    builder.Services.AddHttpApi<IIpApi>();

    #endregion

    #region swagger

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(s =>
    {
        s.SwaggerDoc("Examples", new OpenApiInfo { Title = "Examples",Version = "v1"});

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

    //var serviceList = builder.Services.ToList(); 所有注入的service列表

    // 构建app对象后，开始配置管道
    var app = builder.Build();

    #region 生命周期

    app.Lifetime.ApplicationStarted.Register(() => { Log.Information("ApplicationStarted:启动完成"); });
    app.Lifetime.ApplicationStopping.Register(() =>
    {
        // shutdown会停止，直到下面的语句执行完成
        Log.Warning("ApplicationStopping:正在关闭");
    });
    app.Lifetime.ApplicationStopped.Register(() => { Log.Warning("ApplicationStopped:应用已停止"); });

    #endregion

    // 管道最外层配置traceId
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("TraceId", context.TraceIdentifier);
        await next();
        if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            var result = ResultModel<string>.Error("token验证失败", "请重新登录或刷新页面");
            await context.Response.WriteAsJsonAsync(result);
        }
        else if(context.Response.StatusCode ==(int)HttpStatusCode.Forbidden)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            var result = ResultModel<string>.Error("权限不足", "您没有权限进行此操作");
            await context.Response.WriteAsJsonAsync(result);
        }
    });

    // 处理异常
    app.UseExceptionHandler(b =>
    {
        b.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";

            var exception = context.Features.Get<IExceptionHandlerFeature>();
            if (exception != null)
            {
                var result = ResultModel<string>.Error(exception.Error.Message,exception.Error.StackTrace ?? "");
                await context.Response.WriteAsJsonAsync(result);
            }
        });
    });
    // 获取nginx代理信息
    app.UseForwardedHeaders();

#if (EnableBlazor)
    app.UseWebAssemblyDebugging();
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
#endif

    // 移除掉 /kentxxq.Templates.Aspnetcore 前缀
    // 例如请求 kentxxq.com/kentxxq.Templates.Aspnetcore/api/Demo/GetData 就会变成 kentxxq.com/api/Demo/GetData
    app.UsePathBase(new PathString("/kentxxq.Templates.Aspnetcore"));
    app.UseRouting();

    if (app.Environment.IsDevelopment())
    {
        app.UseCors("all");
        app.UseSwagger();
        app.UseSwaggerUI(u => { u.SwaggerEndpoint("/swagger/Examples/swagger.json", "Examples"); });
    }

    app.UseOpenTelemetryPrometheusScrapingEndpoint();
    app.UseSerilogRequestLogging();

    // 下面开始正式处理请求
    app.UseAuthentication();
    app.UseAuthorization();
    // 限速应该在缓存设置前面。否则返回数据的时候，ratelimiter=>cache 429，用户端也会缓存。
    app.UseRateLimiter();
    app.UseResponseCaching();

#if (EnableBlazor)
    app.MapRazorPages();
#endif

    app.MapControllers();

#if (EnableSignalR)
    app.MapHub<ChatHub>("/chatHub");
#endif

    #region 健康检查管道

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

#if (EnableBlazor)
    app.MapFallbackToFile("index.html");
#endif

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