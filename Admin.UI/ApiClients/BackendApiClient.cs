using System.Net.Http.Json;
using Shared;

namespace Admin.UI.ApiClients;

public class BackendApiClient(HttpClient httpClient)
{
    public async Task Test()
    {
        var response = await httpClient.GetAsync("/test");
        response.EnsureSuccessStatusCode();
    }
    
    public async Task<List<ChatUserEventMessage>> GetHistory()
    {
        //TODO
        return new List<ChatUserEventMessage>();
    }
    
    public async Task<LoginResponse> Login()
    {
        return await httpClient.GetFromJsonAsync<LoginResponse>("/login-admin");
    }
}

public class LoginResponse
{
    public string Url { get; set; }
    public string AccessToken { get; set; }
}