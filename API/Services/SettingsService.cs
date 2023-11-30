using Shared;

namespace API.Services;

public interface ISettingsService
{
    Task AddPage(string id, string text,string content);
    Task DeletePage(string id);
    Task UpdatePage(string id, string text,string content);
    Task<List<KnowledgeBasePage>> GetPages();
    Task SaveSettings(ChatSettings content);
    Task<ChatSettings> GetSettings();
}
public class SettingsService(ICosmosDbService database,IAISearchService searchService,ITextService textService):ISettingsService
{
    public async Task AddPage(string id, string text,string content)
    {
        await database.AddPage(id, text,content);
    }

    public async Task DeletePage(string id)
    {
        await database.DeletePage(id);
    }

    public async Task UpdatePage(string id, string text,string content)
    {
        var page = await database.GetPage(id);
        await database.UpdatePage(id, text, content,textService.GetLinesCount(content));
        await searchService.Save(content, id, page.TotalLines);
    }

    public async Task<List<KnowledgeBasePage>> GetPages()
    {
        return await database.GetPages();
    }

    public async Task SaveSettings(ChatSettings content)
    {
        await database.SaveSettings(content);
    }

    public async Task<ChatSettings> GetSettings()
    {
        var settings = await database.GetSettings();
        if (settings == null)
        {
            return new ChatSettings()
            {
                Id = "SETTINGS",
                Text = ""
            };
        }

        return settings;
    }
}