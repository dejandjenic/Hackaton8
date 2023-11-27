namespace API.Repositories;

public interface IChatRepository
{
    Task<List<ChatHistory>> GetHistory(string userId);
    Task SaveNewChatItem(string userId,string text, bool fromUser);
}