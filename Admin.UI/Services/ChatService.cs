using Shared;

namespace Admin.UI.Services;

public class ChatService(ConnectionService connectionService)
{
    public async Task Init()
    {
        await connectionService.Connect();
    }

    public async Task<IDisposable> RegisterChatEventHandler(Action<ChatUserEventMessage> handler)
    {
        return connectionService.Bind("ChatEvent", handler);
    }
    
    public async Task<IDisposable> RegisterNewChatEventHandler(Action<ChatUserEvent> handler)
    {
        return connectionService.Bind("NewChatEvent", handler);
    }

    public void Watch(string userId)
    {
        connectionService.Send("Watch", userId);
    }
    
    public void UnWatch(string userId)
    {
        connectionService.Send("UnWatch", userId);
    }
}