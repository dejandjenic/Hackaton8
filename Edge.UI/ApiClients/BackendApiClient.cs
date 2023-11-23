using System.Net.Http.Json;

namespace Edge.UI.ApiClients;

public class BackendApiClient(HttpClient httpClient)
{
    public async Task<LoginResponse> Login()
    {
        return await httpClient.GetFromJsonAsync<LoginResponse>("/login");
    }
}

public class LoginResponse
{
    public string Url { get; set; }
    public string AccessToken { get; set; }
}