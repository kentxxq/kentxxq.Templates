using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using kentxxq.Templates.Aspnetcore.Webapi.Common.RateLimitPolicy;
using Microsoft.AspNetCore.RateLimiting;

namespace kentxxq.Templates.Aspnetcore.Webapi.Extensions;

/// <summary>
/// 限速拓展方法
/// </summary>
public static class MyRateLimiterExtension
{
    /// <summary>
    /// 添加限速配置
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static void AddMyRateLimiter(this IServiceCollection service)
    {
        service.AddRateLimiter(o =>
        {
            #region 全局限速配置

            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.OnRejected = (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);

                var info =
                    $"User {context.HttpContext.User.Identity?.Name ?? "Anonymous"}, Endpoint {context.HttpContext.Request.Path}, Ip {context.HttpContext.Connection.RemoteIpAddress?.MapToIPv4()}";
                context.HttpContext.RequestServices.GetService<ILoggerFactory>()?
                    .CreateLogger(nameof(MyRateLimiterExtension))
                    .LogWarning("Limited: {Info}", info);
                return new ValueTask();
            };

            o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, (IPAddress, string)>(context =>
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;
                var username = "anonymous user";
                if (context.User.Identity?.IsAuthenticated is true) username = context.User.ToString()!;

                if (!IPAddress.IsLoopback(remoteIpAddress!))
                    return RateLimitPartition.GetTokenBucketLimiter
                    ((remoteIpAddress!, username), _ =>
                        new TokenBucketRateLimiterOptions
                        {
                            // 类似 PermitLimit 限制总次数
                            TokenLimit = 50,
                            // 每次补充令牌的最大数量
                            TokensPerPeriod = 50,
                            // 刷新周期
                            ReplenishmentPeriod = TimeSpan.FromSeconds(5),
                            // 自动刷新、补充次数
                            AutoReplenishment = true,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 2
                        });
                return RateLimitPartition.GetNoLimiter((IPAddress.Loopback, "localhost"));

                // return RateLimitPartition.GetTokenBucketLimiter((remoteIpAddress!, username), _ =>
                //     new TokenBucketRateLimiterOptions
                //     {
                //         // 类似 PermitLimit 限制总次数
                //         TokenLimit = 1,
                //         // 每次补充令牌的最大数量
                //         TokensPerPeriod = 1,
                //         // 刷新周期
                //         ReplenishmentPeriod = TimeSpan.FromSeconds(5),
                //         // 自动刷新、补充次数
                //         AutoReplenishment = true,
                //         QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                //         QueueLimit = 0
                //     }
                // );
            });

            #endregion

            #region 示例配置

            // 固定
            o.AddFixedWindowLimiter("fixed", fo =>
            {
                // 时间窗口
                fo.Window = TimeSpan.FromSeconds(5);
                // 次数限制
                fo.PermitLimit = 5;
                // 先进先出
                fo.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                // 队列大小
                fo.QueueLimit = 0;
            });

            // 更加平滑
            o.AddSlidingWindowLimiter("sliding", so =>
            {
                // 30秒内最多100个请求，分割成三段。也就是说第40秒的时候，剩余次数+前10秒消耗的次数。
                so.SegmentsPerWindow = 3;
                so.Window = TimeSpan.FromSeconds(30);
                so.PermitLimit = 100;
                so.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                so.QueueLimit = 2;
            });

            o.AddTokenBucketLimiter("token", to =>
            {
                // 类似 PermitLimit 限制总次数
                to.TokenLimit = 100;
                // 每次补充令牌的最大数量
                to.TokensPerPeriod = 50;
                // 刷新周期
                to.ReplenishmentPeriod = TimeSpan.FromSeconds(5);
                // 自动刷新、补充次数
                to.AutoReplenishment = true;
                to.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                to.QueueLimit = 2;
            });

            o.AddConcurrencyLimiter("concurrency", fo =>
            {
                // 最大并发5
                fo.PermitLimit = 5;
                fo.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                fo.QueueLimit = 2;
            });

            #endregion

            #region 自定义的限速策略

            o.AddPolicy<IPAddress, IpRateLimitPolicy>("ip");
            o.AddPolicy<string, UsernameRateLimitPolicy>("username");

            #endregion
        });
    }
}