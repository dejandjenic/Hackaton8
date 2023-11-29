using System.Net;
using System.Text.RegularExpressions;
using API.Configuration;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.Memory.AzureCognitiveSearch;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;

namespace API.Services;

public interface IAISearchService
{
    Task Save(string content, string pageId, int previousLinesCount);
}

public class AISearchService : IAISearchService
{
    private readonly AppSettings _appSettings;
    private readonly ITextService _textService;
    private ISemanticTextMemory memory;

    public AISearchService(AppSettings appSettings,ITextService textService)
    {
        _appSettings = appSettings;
        _textService = textService;
        string aoaiEndpoint = appSettings.OpenAIEndpoint;
        string aoaiApiKey = appSettings.OpenAIKey;
        string acsEndpoint = appSettings.SearchEndpoint;
        string acsApiKey = appSettings.SearchKey;
        string aoaiModel = "chat2";//appSettings.OpenAIModel;
        string collectionName = appSettings.SearchCollectionName;

        memory = new MemoryBuilder()
            .WithLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
            .WithMemoryStore(new AzureCognitiveSearchMemoryStore(acsEndpoint, acsApiKey))
            .WithAzureOpenAITextEmbeddingGenerationService(
                aoaiModel, aoaiEndpoint, aoaiApiKey)
            .Build();
    }
    
    public async Task Save(string content,string pageId,int previousLinesCount)
    {
        string collectionName = _appSettings.SearchCollectionName;

        for (int i = 0; i < previousLinesCount; i++)
        {
            try
            {
                var result = await memory.GetAsync(collectionName, $"{pageId}.{i}");
                if (result != null)
                {
                    await memory.RemoveAsync(collectionName, $"{pageId}.{i}");
                }
            }
            catch{}
        }

        string[] paragraphs = _textService.GetLines(content);
        for (int i = 0; i < paragraphs.Length; i++)
            await memory.SaveInformationAsync(collectionName, paragraphs[i], $"{pageId}.{i}");

    }
}