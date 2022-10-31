using Microsoft.AspNetCore.SignalR;

namespace kentxxq.Templates.Aspnetcore.Webapi.Hubs;

/// <summary>
/// 聊天demo
/// </summary>
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;


    /// <summary>
    /// 依赖注入
    /// </summary>
    /// <param name="logger"></param>
    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 发送信息
    /// </summary>
    /// <param name="user"></param>
    /// <param name="message"></param>
    public async Task SendMessage(string user, string message)
    {
        _logger.LogInformation("user:{User},msg:{Message}", user, message);
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}