namespace Edge.UI.Services;

public class ChatService(ConnectionService connectionService)
{
    public async Task Init()
    {
        await connectionService.Connect();
    }

    public async Task<IDisposable> RegisterGptHandler(Action<string> handler)
    {
        return connectionService.Bind("Respond", handler);
    }

    public async Task SendMessage(string message)
    {
        await connectionService.Send("Message", message);
    }
}