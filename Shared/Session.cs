using Newtonsoft.Json;
using Shared;

namespace API.Models.Cosmos;

public record Session
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; set; }

    public string Type { get; set; }

    /// <summary>
    /// Partition key
    /// </summary>
    public string SessionId { get; set; }
    public string UserId { get; set; }

    public int? TokensUsed { get; set; }

    public string Name { get; set; }

    [JsonIgnore]
    public List<Message> Messages { get; set; }

    public Session(string userId)
    {
        Id = Guid.NewGuid().ToString();
        Type = nameof(Session);
        SessionId = Id;
        UserId = userId;
        TokensUsed = 0;
        Name = "New Chat";
        Messages = new List<Message>();
    }

    public Session()
    {
        Id = Guid.NewGuid().ToString();
        Type = nameof(Session);
        SessionId = Id;
        UserId = "userId";
        TokensUsed = 0;
        Name = "New Chat";
        Messages = new List<Message>();
    }

    public void AddMessage(Message message)
    {
        Messages.Add(message);
    }

}