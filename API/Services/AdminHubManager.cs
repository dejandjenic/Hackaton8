using API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Shared;

namespace API.Services;

public class AdminHubManager(IHubContext<AdminHub,IAdminHub> context,IHubContextStore store):IAdminHubManager
{
    public async Task ChatEvent(ChatUserEvent chatEvent)
    {

        await context.Clients.All.NewChatEvent(chatEvent);
    }

    public async Task ChatUserEvent(string userId, ChatUserEventMessage message)
    {
        await context.Clients.Group(userId).ChatEvent(message);
    }
}