using Shared;

namespace Admin.UI.Services;

public class KnowledgeBaseSettingsService
{
    private static List<KnowledgeBasePage> temp = new();
    private Dictionary<string, List<KnowledgeBaseLine>> lines = new();

    public async Task SetLines(List<KnowledgeBaseLine> existing, List<KnowledgeBaseLine> newlines,string pageId)
    {
        foreach (var line in newlines)
            await AddLine(pageId, line.Text);


        foreach (var line in existing)
        {
            var item = lines[pageId].FirstOrDefault(x => x.Id == line.Id);
            lines[pageId].Remove(item);
            lines[pageId].Add(new KnowledgeBaseLine(line.Text,pageId,line.Id));
        }
    }
    private async Task AddLine(string pageId,string text)
    {
        var id = Guid.NewGuid().ToString();
        if (lines.ContainsKey(pageId))
        {
            lines[pageId].Add(new KnowledgeBaseLine(text, pageId, id));
        }
        else
        {
            lines.Add(pageId,new List<KnowledgeBaseLine>()
            {
                 new (text,pageId,id)
            });
        }
    }
    
    private async Task UpdateLine(string id, string text)
    {
        var item = lines.SelectMany(x => x.Value).FirstOrDefault(x => x.Id == id);
        lines[item.PageId].Remove(item);
        lines[item.PageId].Add(new KnowledgeBaseLine(text,item.PageId,id));
    }
    
    public async Task<List<KnowledgeBaseLine>> GetLines(string pageId)
    {
        if(lines.ContainsKey(pageId))
            return lines[pageId];
        return new List<KnowledgeBaseLine>();
    }

    public async Task<string> AddPage(string text)
    {
        var id = Guid.NewGuid().ToString();
        temp.Add(new KnowledgeBasePage(text,id));
        return id;
    }
    
    public async Task UpdatePage(string id, string text)
    {
        var page = temp.FirstOrDefault(x => x.Id == id);
        temp.Remove(page);
        temp.Add(new KnowledgeBasePage(text,id));
    }
    
    public async Task<List<KnowledgeBasePage>> GetPages()
    {
        return temp;
    }
}