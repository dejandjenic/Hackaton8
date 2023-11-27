namespace API.Hubs;

public interface IChatHub
{
    Task Respond(string message);
}