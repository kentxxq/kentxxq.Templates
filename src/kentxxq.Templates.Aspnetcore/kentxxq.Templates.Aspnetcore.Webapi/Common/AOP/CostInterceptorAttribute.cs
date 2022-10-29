using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;

namespace kentxxq.Templates.Aspnetcore.Webapi.Common.AOP
{
    /// <summary>
    /// 服务耗时标签
    /// </summary>
    public class CostInterceptorAttribute : AbstractInterceptorAttribute
    {
        [FromServiceContext]
        public ILogger<CostInterceptorAttribute> _logger { get; set; }
        private DateTime _beginTime;

        /// <inheritdoc/>
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            try
            {
                _beginTime = DateTime.Now;
                await next(context);
            }
            catch (Exception)
            {
                //Console.WriteLine("Service threw an exception!");
                throw;
            }
            finally
            {
                TimeSpan cost = DateTime.Now - _beginTime;
                _logger.LogInformation($"计算{context.Implementation}.{context.ProxyMethod.Name}耗时{cost.TotalMilliseconds}ms");
            }
        }
    }
}
