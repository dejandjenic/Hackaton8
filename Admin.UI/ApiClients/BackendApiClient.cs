namespace Admin.UI.ApiClients;

public class BackendApiClient(HttpClient httpClient)
{
    public async Task Test()
    {
        var response = await httpClient.GetAsync("/test");
        response.EnsureSuccessStatusCode();
    }
}