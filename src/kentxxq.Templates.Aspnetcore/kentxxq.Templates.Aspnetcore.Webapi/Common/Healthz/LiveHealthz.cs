using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace kentxxq.Templates.Aspnetcore.Webapi.Common.Healthz;

/// <summary>
/// 存活检查
/// </summary>
public class LiveHealthz : IHealthCheck
{
    /// <summary>
    /// 存活检查逻辑
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new())
    {
        return Task.FromResult(HealthCheckResult.Healthy("存活中..."));
        //return Task.FromResult(HealthCheckResult.Unhealthy("挂了..."));
    }
}