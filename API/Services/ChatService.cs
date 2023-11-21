using API.Repositories;

namespace API.Services;

public interface IChatService
{
    Task<List<ChatHistory>> GetHistory(string userId);
    Task SaveNewChatItem(string userId,string text, bool fromUser);
}

public class ChatService(IChatRepository chatRepository) : IChatService
{
    public async Task<List<ChatHistory>> GetHistory(string userId)
    {
        return await chatRepository.GetHistory(userId);
    }

    public async Task SaveNewChatItem(string userId, string text, bool fromUser)
    {
        await chatRepository.SaveNewChatItem(userId, text, fromUser);
    }
}