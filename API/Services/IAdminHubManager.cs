using Shared;

namespace API.Services;

public interface IAdminHubManager
{
    Task ChatEvent(ChatUserEvent chatEvent);
    Task ChatUserEvent(string userId, ChatUserEventMessage message);
}