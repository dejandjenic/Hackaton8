namespace API.Repositories;

public class ChatHistory
{
    public bool FromUser { get; set; }
    public string Text { get; set; }
    public DateTime Date { get; set; }
}