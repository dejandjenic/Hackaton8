namespace Shared;

public class ChatUserEvent
{
    public string UserId { get; set; }
}

public class ChatUserEventMessage
{
    public bool FromUser { get; set; }
    public string Message { get; set; }
    public string UserId { get; set; }
}

public record KnowledgeBasePage(string Name,string Id);

public class KnowledgeBaseLine
{
    public KnowledgeBaseLine(string text,string pageid,string id)
    {
        Text = text;
        PageId = pageid;
        Id = id;
    }
    public string Text { get; set; }
    public string PageId { get; set; }
    public string Id { get; set; }
}