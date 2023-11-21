using API.Services;
using Microsoft.AspNetCore.SignalR;

public class Chat(IGPTService gptService) : Hub
{
    public void BroadcastMessage(string name, string message)
    {
        Clients.All.SendAsync("broadcastMessage", name, message);
    }

    public void Message(string name, string message)
    {
        Clients.Client(Context.ConnectionId).SendAsync("gpt", gptService.Respond("temp",message).GetAwaiter().GetResult());
    }
}