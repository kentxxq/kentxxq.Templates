using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace kentxxq.Templates.Aspnetcore.Webapi.Common.RateLimitPolicy;

/// <summary>
/// ip限速
/// </summary>
public class IpRateLimitPolicy : IRateLimiterPolicy<IPAddress>
{
    private readonly ILogger<UsernameRateLimitPolicy> _logger;

    /// <summary>
    /// 依赖注入
    /// </summary>
    /// <param name="logger"></param>
    public IpRateLimitPolicy(ILogger<UsernameRateLimitPolicy> logger)
    {
        _logger = logger;
        OnRejected = (ctx, token) =>
        {
            ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            _logger.LogWarning($"Request rejected by {nameof(IpRateLimitPolicy)}");
            return ValueTask.CompletedTask;
        };
    }

    /// <inheritdoc />
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; }

    /// <inheritdoc />
    public RateLimitPartition<IPAddress> GetPartition(HttpContext httpContext)
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if (!IPAddress.IsLoopback(remoteIp!))
            return RateLimitPartition.GetFixedWindowLimiter(remoteIp!, _ => new FixedWindowRateLimiterOptions
            {
                // 时间窗口
                Window = TimeSpan.FromSeconds(5),
                // 次数限制
                PermitLimit = 100,
                // 先进先出
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                // 队列大小
                QueueLimit = 2
            });
        return RateLimitPartition.GetNoLimiter(IPAddress.Loopback);
    }
}