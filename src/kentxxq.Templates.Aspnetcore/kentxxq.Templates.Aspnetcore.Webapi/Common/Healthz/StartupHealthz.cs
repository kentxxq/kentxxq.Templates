using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace kentxxq.Templates.Aspnetcore.Webapi.Common.Healthz;

/// <summary>
/// 启动健康检查
/// </summary>
public class StartupHealthz : IHealthCheck
{
    private bool _isCheck = false;

    /// <summary>
    /// 健康检查逻辑
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        return Task.FromResult(HealthCheckResult.Healthy("启动完成..."));
    }
}