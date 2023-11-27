namespace API.Services;

public interface IUserHubManager
{
    Task Respond(string userId, string message);
}