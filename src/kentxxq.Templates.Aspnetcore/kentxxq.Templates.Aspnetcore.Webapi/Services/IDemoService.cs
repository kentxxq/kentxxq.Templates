using EasyCaching.Core.Interceptor;

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
    string GetData();

    /// <summary>
    /// 获取缓存数据
    /// </summary>
    /// <returns></returns>
#if (EnableRedis)
    [EasyCachingAble(CacheProviderName = "redis1", Expiration = 10)]
#else
    [EasyCachingAble(CacheProviderName = "memory1", Expiration = 10)]
#endif
    Task<DateTime> GetCacheData();
}