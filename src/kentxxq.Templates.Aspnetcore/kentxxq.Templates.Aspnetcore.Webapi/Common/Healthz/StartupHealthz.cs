using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace kentxxq.Templates.Aspnetcore.Webapi.Common.Healthz;

/// <summary>
/// 启动健康检查
/// </summary>
public class StartupHealthz : IHealthCheck
{
    private volatile bool _isReady;

    /// <summary>
    /// 是否启动完成
    /// </summary>
    public bool StartupCompleted
    {
        get => _isReady;
        set => _isReady = value;
    }

    /// <summary>
    /// 健康检查逻辑
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        if (StartupCompleted) return Task.FromResult(HealthCheckResult.Healthy("启动完成"));
        return Task.FromResult(HealthCheckResult.Unhealthy("还未启动完成"));
    }
}