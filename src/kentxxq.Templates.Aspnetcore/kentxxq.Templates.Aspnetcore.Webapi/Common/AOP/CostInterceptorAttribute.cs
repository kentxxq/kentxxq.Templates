using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;

namespace kentxxq.Templates.Aspnetcore.Webapi.Common.AOP;

/// <summary>
/// 服务耗时标签
/// </summary>
public class CostInterceptorAttribute : AbstractInterceptorAttribute
{
    private DateTime _beginTime;

    [FromServiceContext] public ILogger<CostInterceptorAttribute> _logger { get; set; }

    /// <inheritdoc />
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        try
        {
            _beginTime = DateTime.Now;
            await next(context);
        }
        finally
        {
            var cost = DateTime.Now - _beginTime;
            _logger.LogInformation(
                $"计算{context.Implementation}.{context.ProxyMethod.Name}耗时{cost.TotalMilliseconds}ms");
        }
    }
}