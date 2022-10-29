using EasyCaching.Core.Interceptor;
using kentxxq.Templates.Aspnetcore.Webapi.Common.AOP;

namespace kentxxq.Templates.Aspnetcore.Webapi.Services;

/// <summary>
/// demo服务
/// </summary>
public interface IDemoService
{
    /// <summary>
    /// 获取demo数据
    /// </summary>
    /// <returns></returns>
    [CostInterceptor]
    Task<string> GetData();

    /// <summary>
    /// 获取本地缓存数据
    /// </summary>
    /// <returns></returns>
    [EasyCachingAble(CacheProviderName = "memory1", Expiration = 10)]
    Task<DateTime> GetLocalCacheData();

#if (EnableRedis)
    /// <summary>
    /// 获取redis缓存数据
    /// </summary>
    /// <returns></returns>
    [EasyCachingAble(CacheProviderName = "redis1", Expiration = 10)]
    Task<DateTime> GetRedisCacheData();
#endif
}