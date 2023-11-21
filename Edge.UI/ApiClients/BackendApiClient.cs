using System.Net.Http.Json;

namespace Edge.UI.ApiClients;

public class BackendApiClient
{
    private readonly HttpClient _httpClient;

    public BackendApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponse> Login()
    {
        return await _httpClient.GetFromJsonAsync<LoginResponse>("/login");
    }
}

public class LoginResponse
{
    public string Url { get; set; }
    public string AccessToken { get; set; }
}