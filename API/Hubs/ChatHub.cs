using System.Security.Claims;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

[Authorize]
public class ChatHub : Hub<IChatHub>
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }
    public override Task OnConnectedAsync()
    {
        var userId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
        Groups.AddToGroupAsync(Context.ConnectionId, userId);
        return base.OnConnectedAsync();
    }

    public async Task Message(string? sessionId, string message)
    {
        var userId = Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

        await _chatService.GetChatCompletionAsync(sessionId, message, userId);
    }
}