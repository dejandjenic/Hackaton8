using API.Services;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace API.Hubs;

public class AdminHub(IGPTService gptService) : Hub<IAdminHub>
{
    public void Watch(string userId)
    {
        Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }

    public void UnWatch(string userId)
    {
        Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
    }
    public async Task Message(string userId, string message)
    {
        await gptService.RespondAsAdmin(userId, message);
    }
}

