namespace API.Repositories;

public interface IChatRepository
{
    Task<List<ChatHistory>> GetHistory(string userId);
    Task SaveNewChatItem(string userId,string text, bool fromUser);
}

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

public class ChatHistory
{
    public bool FromUser { get; set; }
    public string Text { get; set; }
    public DateTime Date { get; set; }
}