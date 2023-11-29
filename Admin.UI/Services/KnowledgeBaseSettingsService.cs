using Admin.UI.ApiClients;
using Shared;

namespace Admin.UI.Services;

public class KnowledgeBaseSettingsService(BackendApiClient apiClient)
{
    private static List<KnowledgeBasePage> temp = new();
    public async Task<string> AddPage(string text,string content)
    {
        var id = Guid.NewGuid().ToString();
        //temp.Add(new KnowledgeBasePage(text,id));
        await apiClient.AddPage(id,text,content);
        return id;
    }
    
    public async Task UpdatePage(string id, string text,string content)
    {
        var page = temp.FirstOrDefault(x => x.Id == id);
        //temp.Remove(page);
        //temp.Add(new KnowledgeBasePage(text,id));
        await apiClient.UpdatePage(id,text,content);
    }
    
    public async Task<List<KnowledgeBasePage>> GetPages()
    {
        return await apiClient.GetPages();
    }
}