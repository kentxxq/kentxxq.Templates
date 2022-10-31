namespace kentxxq.Templates.Aspnetcore.Webapi.Common.Healthz;

/// <inheritdoc />
public class StartupBackgroundService : BackgroundService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<StartupBackgroundService> _logger;
    private readonly StartupHealthz _startupHealthz;

    /// <summary>
    /// 依赖注入
    /// </summary>
    /// <param name="startupHealthz"></param>
    /// <param name="hostApplicationLifetime"></param>
    /// <param name="logger"></param>
    public StartupBackgroundService(StartupHealthz startupHealthz, IHostApplicationLifetime hostApplicationLifetime,
        ILogger<StartupBackgroundService> logger)
    {
        _startupHealthz = startupHealthz;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }

    /// <summary>
    /// 检查完成后设置startup状态
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _hostApplicationLifetime.ApplicationStarted.Register(() =>
        {
            _startupHealthz.StartupCompleted = true;
        });
        await Task.Yield();
    }
}