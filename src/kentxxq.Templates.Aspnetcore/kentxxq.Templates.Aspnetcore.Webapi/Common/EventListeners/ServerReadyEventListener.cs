using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace kentxxq.Templates.Aspnetcore.Webapi.Common.EventListeners;

/// <summary>
/// 检测Microsoft.AspNetCore.Hosting的ServerReady事件
/// 参考 https://github.com/dotnet/aspnetcore/blob/main/src/Hosting/Hosting/src/Internal/HostingEventSource.cs
/// </summary>
public class ServerReadyEventListener : EventListener
{
    /// <inheritdoc />
    protected override void OnEventSourceCreated(EventSource source)
    {
        // 输出程序所有的事件源 Console.WriteLine(source.Name);
        if (source.Name == "Microsoft.AspNetCore.Hosting")
        {
            EnableEvents(source, EventLevel.Verbose, EventKeywords.All, new Dictionary<string, string>
            {
                ["EventCounterIntervalSec"] = "1"
            }!);
        }
    }

    /// <inheritdoc />
    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventName != "ServerReady")
        {
            return;
        }
        // eventData.Payload 和 eventId 等信息都很有用
        Console.WriteLine("检测到ServerReady事件");
    }
}