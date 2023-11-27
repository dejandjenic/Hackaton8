using API.Repositories;

namespace API.Services;

public interface IChatService
{
    Task<List<ChatHistory>> GetHistory(string userId);
    Task SaveNewChatItem(string userId,string text, bool fromUser);
}