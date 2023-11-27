using API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Shared;

namespace API.Services;

public class UserHubManager(IHubContext<ChatHub,IChatHub> context):IUserHubManager
{
    public async Task Respond(string userId, string message)
    {
        await context.Clients.Group(userId).Respond(message);
    }
}