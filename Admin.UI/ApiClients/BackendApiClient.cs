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

    public async Task<List<ChatUserEventMessage>> GetHistory(string userId)
    {
        return await httpClient.GetFromJsonAsync<List<ChatUserEventMessage>>($"/chat-history/{userId}");
    }

    public async Task<LoginResponse> Login()
    {
        return await httpClient.GetFromJsonAsync<LoginResponse>("/login-admin");
    }

    public async Task AddPage(string id, string text, string content)
    {
        await httpClient.PostAsJsonAsync($"/pages", new KnowledgeBasePage(text, id, content));
    }

    public async Task UpdatePage(string id, string text, string content)
    {
        await httpClient.PatchAsJsonAsync($"/pages/{id}", new KnowledgeBasePage(text, id, content));
    }

    public async Task<List<KnowledgeBasePage>> GetPages()
    {
        return await httpClient.GetFromJsonAsync<List<KnowledgeBasePage>>("/pages");
    }

    public async Task SaveSettings(ChatSettings settings)
    {
        await httpClient.PostAsJsonAsync($"/settings", settings);
    }

    public async Task<ChatSettings> GetSettings()
    {
        return await httpClient.GetFromJsonAsync<ChatSettings>($"/settings");
    }
    public async Task PauseChat(string id, bool pause)
    {
        await httpClient.PostAsJsonAsync($"/pause-chat/{id}/{pause}", new object());
    }
}

public class LoginResponse
{
    public string Url { get; set; }
    public string AccessToken { get; set; }
}