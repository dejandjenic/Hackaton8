namespace Shared;

public class ChatUserEvent
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public bool IsPaused { get; set; }
}

public class ChatUserEventMessage
{
    public bool FromUser { get; set; }
    public string Text { get; set; }
    public string UserId { get; set; }
}

public class ChatUser
{
    public string Id { get; set; }
    public bool Active { get; set; }
    public DateTime FirstInteraction { get; set; }
    public DateTime LastInteraction { get; set; }
    public string ChatName { get; set; } = "New Chat";
    public bool ChatPaused { get; set; } = false;
    public string Type => nameof(ChatUser);
}

public class KnowledgeBasePage
{
    public string Name { get; set; }
    public string Content { get; set; }
    public string Id { get; set; }
    public int TotalLines { get; set; }

    public KnowledgeBasePage()
    {

    }

    public KnowledgeBasePage(string name, string id, string content)
    {
        Name = name;
        Id = id;
        Content = content;
    }

    public KnowledgeBasePage(string name, string id, string content, int lines)
    {
        Name = name;
        Id = id;
        Content = content;
        TotalLines = lines;
    }

    public string Type => nameof(KnowledgeBasePage);
}

public class ChatSettings
{
    public string Id { get; set; }
    public string Text { get; set; }
    public string Type => nameof(ChatSettings);
}
    