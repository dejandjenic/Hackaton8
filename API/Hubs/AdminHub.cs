using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

public class AdminHub : Hub<IAdminHub>
{
    public void Watch(string userId)
    {
        Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }
    
    public void UnWatch(string userId)
    {
        Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
    }
}

