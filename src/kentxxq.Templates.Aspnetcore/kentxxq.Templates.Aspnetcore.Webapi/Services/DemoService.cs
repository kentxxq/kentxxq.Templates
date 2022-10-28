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
    public async Task<DateTime> GetCacheData()
    {
        await Task.Delay(2);
        return DateTime.Now;
    }
}