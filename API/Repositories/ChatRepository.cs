namespace API.Repositories;

public class ChatRepository : IChatRepository
{
    private List<ChatHistory> list = new ();
    public async Task<List<ChatHistory>> GetHistory(string userId)
    {
        return list;
    }

    public async Task SaveNewChatItem(string userId, string text, bool fromUser)
    {
        list.Add(new ChatHistory()
        {
            Date = DateTime.UtcNow,
            FromUser = fromUser,
            Text = text
        });
    }
}