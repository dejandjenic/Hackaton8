using API.Repositories;

namespace API.Services;

public class ChatService(IChatRepository chatRepository) : IChatService
{
    public async Task<List<ChatHistory>> GetHistory(string userId)
    {
        return await chatRepository.GetHistory(userId);
    }

    public async Task SaveNewChatItem(string userId, string text, bool fromUser, string role)
    {
        await chatRepository.SaveNewChatItem(userId, text, fromUser, role);
    }
}