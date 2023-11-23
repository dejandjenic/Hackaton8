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