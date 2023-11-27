using API.Models.Cosmos;
using System.Net.Http.Json;
using Shared;

namespace Edge.UI.ApiClients;

public class BackendApiClient(HttpClient httpClient)
{
    public async Task<LoginResponse> Login()
    {
        return await httpClient.GetFromJsonAsync<LoginResponse>("/login");
    }
    public async Task<List<Session>> ChatHistory()
    {
        var userId = "52a4ee72-f039-4a53-8300-aaddf10f8995"; //todo
        return await httpClient.GetFromJsonAsync<List<Session>>($"/chat-history/{userId}");
    }

    public async Task<List<Message>> ChatMessages(string sessionId)
    {
        return await httpClient.GetFromJsonAsync<List<Message>>($"/chat-history-session/{sessionId}");
    }

    public async Task<string> ChatTitle(string? sessionId, string conversationText)
    {
        var response = await httpClient.GetFromJsonAsync<Title>($"/chat-title/{sessionId}?text={conversationText}");
        return response.Text;
    }

    public async Task<Session> Session(string userId)
    {
        //todo userid
        return await httpClient.GetFromJsonAsync<Session>($"/new-session/{userId}");
    }
}

public class LoginResponse
{
    public string Url { get; set; }
    public string AccessToken { get; set; }
}
public class Title
{
    public string Text { get; set; }
}