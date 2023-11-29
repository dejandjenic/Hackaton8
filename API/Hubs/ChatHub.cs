using System.Security.Claims;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

[Authorize]
public class ChatHub(IGPTService gptService,ICosmosDbService database) : Hub<IChatHub>
{
    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
        await database.UpdateUserActive(userId,false);
    }

    public async override Task OnConnectedAsync()
    {
        var userId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
        Groups.AddToGroupAsync(Context.ConnectionId, userId);
        await database.UpdateUserActive(userId,true);
    }

    public async Task Message(string message)
    {    
        var userId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
        await database.UpdateUserLastInteraction(userId);
        await gptService.Respond(userId, message);
    }
}