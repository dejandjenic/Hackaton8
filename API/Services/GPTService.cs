using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using API.Configuration;
using API.Repositories;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.Memory.AzureCognitiveSearch;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;

namespace API.Services;

public interface IGPTService
{
    Task Respond(string userId, string message);
}

public class GPTService(IChatService chatService, AppSettings appSettings, IHubContext<ChatHub,IChatHub> hubContext) : IGPTService
{
    public async Task Respond(string userId, string message)
    {
        string aoaiEndpoint = appSettings.OpenAIEndpoint;
        string aoaiApiKey = appSettings.OpenAIKey;
        string acsEndpoint = appSettings.SearchEndpoint;
        string acsApiKey = appSettings.SearchKey;
        string aoaiModel = appSettings.OpenAIModel;
        string collectionName = appSettings.SearchCollectionName;

        var aoai = new OpenAIClient(new Uri(aoaiEndpoint), new AzureKeyCredential(aoaiApiKey));
        // ISemanticTextMemory memory = new MemoryBuilder()
        //     .WithLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
        //     .WithMemoryStore(new AzureCognitiveSearchMemoryStore(acsEndpoint, acsApiKey))
        //     .WithAzureOpenAITextEmbeddingGenerationService(
        //         aoaiModel, aoaiEndpoint, aoaiApiKey)
        //     .Build();
        // IList<string> collections = await memory.GetCollectionsAsync();
        //
        // if (collections.Contains(collectionName))
        // {
        //     Console.WriteLine("Found database");
        // }
        // else
        // {
        //     using HttpClient client = new();
        //     string s = await client.GetStringAsync(
        //         "https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7");
        //     List<string> paragraphs =
        //         TextChunker.SplitPlainTextParagraphs(
        //             TextChunker.SplitPlainTextLines(
        //                 WebUtility.HtmlDecode(Regex.Replace(s, @"<[^>]+>|&nbsp;", "")),
        //                 128),
        //             1024);
        //     for (int i = 0; i < paragraphs.Count; i++)
        //         await memory.SaveInformationAsync(collectionName, paragraphs[i], $"paragraph{i}");
        //     Console.WriteLine("Generated database");
        // }

        var history = await chatService.GetHistory(userId);


        StringBuilder builder = new();
        var chatCompletionsOptions = new ChatCompletionsOptions();
        chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.System,
            "You are an AI assistant that helps people find information."));
        foreach (var item in history)
        {
            chatCompletionsOptions.Messages.Add(new ChatMessage(
                item.FromUser ? ChatRole.User : ChatRole.Assistant,
                item.Text));
        }

        chatCompletionsOptions.AzureExtensionsOptions = new AzureChatExtensionsOptions
        {
            Extensions =
            {
                new AzureCognitiveSearchChatExtensionConfiguration
                {
                    SearchEndpoint = new Uri(acsEndpoint),
                    SearchKey = new AzureKeyCredential(acsApiKey),
                    IndexName = collectionName,
                    ShouldRestrictResultScope = false,
                }
            }
        };

        var question = $"Question: {message}";//?

        chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, question));
        await chatService.SaveNewChatItem(userId, question,true);

        builder.Clear();
        var response = await aoai.GetChatCompletionsStreamingAsync(aoaiModel, chatCompletionsOptions);

        await foreach (StreamingChatChoice choice in response.Value.GetChoicesStreaming())
        {
            await foreach (ChatMessage item in choice.GetMessageStreaming())
            {
                builder.Append(item.Content);
            }
        }

        var answer = builder.ToString();
        await chatService.SaveNewChatItem(userId, answer,false);
        chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, answer));
        await hubContext.Clients.Group(userId).Respond(answer);
    }
}