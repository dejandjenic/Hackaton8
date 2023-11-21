namespace Edge.UI.Services;

public class ChatService(ConnectionService connectionService)
{
    public async Task Init()
    {
        await connectionService.Connect();
    }

    public async Task RegisterGptHandler(Action<string> handler)
    {
        connectionService.Bind("gpt", handler);
    }

    public async Task SendMessage(string message)
    {
        await connectionService.Send("Message", "user", message);
    }
}