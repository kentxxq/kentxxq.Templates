using kentxxq.Templates.Aspnetcore.Webapi.Common.EventListeners;

namespace kentxxq.Templates.Aspnetcore.Webapi.Extensions;

/// <summary>
/// 注册eventListener事件-拓展方法
/// </summary>
public static class MyEventListenerExtension
{
    /// <summary>
    /// 注册eventListener事件
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddMyEventListener(this IServiceCollection service)
    {
        var serverReadyEventListener = new ServerReadyEventListener();
        service.AddSingleton(serverReadyEventListener);
        return service;
    }
}