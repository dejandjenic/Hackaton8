namespace API.Services;

public interface IGPTService
{
    Task<string> Respond(string userId, string message);
}

public class GPTService:IGPTService
{
    public async Task<string> Respond(string userId,string message)
    {
        return $"echo {message}";
    }
}