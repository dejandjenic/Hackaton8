namespace API.Configuration;

public class AppSettings
{
    public string SignalRConnectionString { get; set; }
    public string OpenAIEndpoint { get; set; }
    public string OpenAIKey { get; set; }
    public string SearchEndpoint { get; set; }
    public string SearchKey { get; set; }
    public string OpenAIModel { get; set; } = "chat3";
    public string SearchCollectionName { get; set; } = "net7perf";
}
