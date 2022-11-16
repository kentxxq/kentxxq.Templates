using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace kentxxq.Templates.Aspnetcore.Webapi.Common.RateLimitPolicy;

/// <summary>
/// 用户限速
/// </summary>
public class UsernameRateLimitPolicy:IRateLimiterPolicy<string>
{
    private readonly ILogger<UsernameRateLimitPolicy> _logger;

    /// <inheritdoc />
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; }

    /// <summary>
    /// 依赖注入
    /// </summary>
    /// <param name="logger"></param>
    public UsernameRateLimitPolicy(ILogger<UsernameRateLimitPolicy> logger)
    {
        _logger = logger;
        OnRejected = (ctx, token) =>
        {
            ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            _logger.LogWarning($"Request rejected by {nameof(UsernameRateLimitPolicy)}");
            return ValueTask.CompletedTask;
        };
    }

    /// <inheritdoc />
    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var username = "anonymous user";
        if (httpContext.User.Identity?.IsAuthenticated is true)
        {
            username = httpContext.User.ToString()!;
        }

        return RateLimitPartition.GetFixedWindowLimiter(username, _ =>new FixedWindowRateLimiterOptions()
        {
            // 时间窗口
            Window = TimeSpan.FromSeconds(5),
            // 次数限制
            PermitLimit = 50,
            // 先进先出
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            // 队列大小
            QueueLimit = 2,
        });
    }
}