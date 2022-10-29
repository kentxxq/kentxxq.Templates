namespace kentxxq.Templates.Aspnetcore.Webapi.Services;

/// <inheritdoc />
public class DemoService : IDemoService
{
    /// <inheritdoc />
    public string GetData()
    {
        return "demo";
    }

    /// <inheritdoc />
    public async Task<DateTime> GetLocalCacheData()
    {
        await Task.Delay(2);
        return DateTime.Now;
    }

#if (EnableRedis)
    /// <inheritdoc />
    public async Task<DateTime> GetRedisCacheData()
    {
        await Task.Delay(2);
        return DateTime.Now;
    }
#endif
}