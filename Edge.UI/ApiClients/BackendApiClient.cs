using Edge.UI.Services;

namespace Edge.UI.ApiClients;

public class BackendApiClient(HttpClient httpClient)
{
	public async Task<LoginResponse> Login()
	{
		return await httpClient.GetFromJsonAsync<LoginResponse>("/login");
	}

	public async Task<List<ChatMessage>> ChatHistoryMessages()
	{
		return await httpClient.GetFromJsonAsync<List<ChatMessage>>("/chat-history");
	}
}

public class LoginResponse
{
	public string Url { get; set; }
	public string AccessToken { get; set; }
}