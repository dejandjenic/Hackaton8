using API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace API.Services;

public interface IAdminHubManager
{
    Task ChatEvent(string userId,object data);
}

public class AdminHubManager(IHubContext<AdminHub,IAdminHub> context,IHubContextStore store):IAdminHubManager
{
    public async Task ChatEvent(string userId,object data)
    {
        
        var x = store.AdminHubContext.Clients.Group(userId);//.ChatEvent(data);
    }
}