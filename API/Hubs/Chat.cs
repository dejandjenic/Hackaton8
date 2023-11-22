using System.Security.Claims;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

public interface IChatHub
{
    Task Respond(string message);
}
[Authorize]
public class ChatHub(IGPTService gptService) : Hub<IChatHub>
{
    public override Task OnConnectedAsync()
    {
        var userId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
        Groups.AddToGroupAsync(Context.ConnectionId, userId);
        return base.OnConnectedAsync();
    }

    public async Task Message(string message)
    {    
        var userId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

        await gptService.Respond(userId, message);
        //Clients.Client(Context.ConnectionId).SendAsync("gpt", gptService.Respond("temp",message).GetAwaiter().GetResult());
    }
}