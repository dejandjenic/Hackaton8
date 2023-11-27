using API.Configuration;
using Azure;
using Azure.AI.OpenAI;
using Shared;

namespace API.Services;

public interface IGPTService
{
    Task Respond(string userId, string message);
    Task<(string completionText, int completionTokens)> GetChatCompletionAsync(string sessionId, string userPrompt, string userId);
    Task<string> SummarizeAsync(string sessionId, string conversationText);
}

public class GPTService : IGPTService
{
    private readonly AppSettings _appSettings;
    private readonly IUserHubManager _userHubManager;
    private readonly IAdminHubManager _adminHubManager;

    private readonly string _modelName = String.Empty;
    private readonly OpenAIClient _client;

    /// <summary>
    /// System prompt to send with user prompts to instruct the model for chat session
    /// </summary>
    private readonly string _systemPrompt = @"
        You are an AI assistant that helps people find information.
        Provide concise answers that are polite and professional." + Environment.NewLine;

    /// <summary>    
    /// System prompt to send with user prompts to instruct the model for summarize
    /// </summary>
    private readonly string _summarizePrompt = @"
        Summarize this prompt in one or two words to use as a label in a button on a web page.
        Do not use any punctuation." + Environment.NewLine;

    public GPTService(AppSettings appSettings, IUserHubManager userHubManager, IAdminHubManager adminHubManager)
    {
        _appSettings = appSettings;
        _userHubManager = userHubManager;
        _adminHubManager = adminHubManager;

        _modelName = appSettings.OpenAIModel;
        _client = new OpenAIClient(new Uri(appSettings.OpenAIEndpoint), new AzureKeyCredential(appSettings.OpenAIKey));
    }
    public async Task Respond(string userId, string message)
    {
        // await adminHubManager.ChatUserEvent(userId,new ChatUserEventMessage
        // {
        //     Message = message,
        //     FromUser = true,
        //     UserId = userId
        // });
        //
        // string aoaiEndpoint = appSettings.OpenAIEndpoint;
        // string aoaiApiKey = appSettings.OpenAIKey;
        // string acsEndpoint = appSettings.SearchEndpoint;
        // string acsApiKey = appSettings.SearchKey;
        // string aoaiModel = appSettings.OpenAIModel;
        // string collectionName = appSettings.SearchCollectionName;
        //
        // var aoai = new OpenAIClient(new Uri(aoaiEndpoint), new AzureKeyCredential(aoaiApiKey));
        // var useRealChat = true;
        //
        // // ISemanticTextMemory memory = new MemoryBuilder()
        // //     .WithLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
        // //     .WithMemoryStore(new AzureCognitiveSearchMemoryStore(acsEndpoint, acsApiKey))
        // //     .WithAzureOpenAITextEmbeddingGenerationService(
        // //         aoaiModel, aoaiEndpoint, aoaiApiKey)
        // //     .Build();
        // // IList<string> collections = await memory.GetCollectionsAsync();
        // //
        // // if (collections.Contains(collectionName))
        // // {
        // //     Console.WriteLine("Found database");
        // // }
        // // else
        // // {
        // //     using HttpClient client = new();
        // //     string s = await client.GetStringAsync(
        // //         "https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7");
        // //     List<string> paragraphs =
        // //         TextChunker.SplitPlainTextParagraphs(
        // //             TextChunker.SplitPlainTextLines(
        // //                 WebUtility.HtmlDecode(Regex.Replace(s, @"<[^>]+>|&nbsp;", "")),
        // //                 128),
        // //             1024);
        // //     for (int i = 0; i < paragraphs.Count; i++)
        // //         await memory.SaveInformationAsync(collectionName, paragraphs[i], $"paragraph{i}");
        // //     Console.WriteLine("Generated database");
        // // }
        //
        // var history = await chatService.GetHistory(userId);
        // var answer = "";
        //
        // StringBuilder builder = new();
        // if (useRealChat)
        // {
        //
        //     var chatCompletionsOptions = new ChatCompletionsOptions();
        //     chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.System,
        //         "You are an AI assistant that helps people find information."));
        //     foreach (var item in history)
        //     {
        //         chatCompletionsOptions.Messages.Add(new ChatMessage(
        //             item.FromUser ? ChatRole.User : ChatRole.Assistant,
        //             item.Text));
        //     }
        //
        //     chatCompletionsOptions.AzureExtensionsOptions = new AzureChatExtensionsOptions
        //     {
        //         Extensions =
        //         {
        //             new AzureCognitiveSearchChatExtensionConfiguration
        //             {
        //                 SearchEndpoint = new Uri(acsEndpoint),
        //                 SearchKey = new AzureKeyCredential(acsApiKey),
        //                 IndexName = collectionName,
        //                 ShouldRestrictResultScope = false,
        //             }
        //         }
        //     };
        //
        //     var question = message;
        //
        //     chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, question));
        //     await chatService.SaveNewChatItem(userId, question, true);
        //
        //     builder.Clear();
        //     var response = await aoai.GetChatCompletionsStreamingAsync(aoaiModel, chatCompletionsOptions);
        //
        //     await foreach (StreamingChatChoice choice in response.Value.GetChoicesStreaming())
        //     {
        //         await foreach (ChatMessage item in choice.GetMessageStreaming())
        //         {
        //             builder.Append(item.Content);
        //         }
        //     }
        //
        //     answer = builder.ToString();
        //     chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, answer));
        // }
        // else
        // {
        //     answer = "no gpt";
        // }
        //
        //
        // await chatService.SaveNewChatItem(userId, answer,false);
        //
        // await userHubManager.Respond(userId,answer);
        // await adminHubManager.ChatEvent(new ChatUserEvent
        // {
        //    UserId = userId
        // });
        // await adminHubManager.ChatUserEvent(userId,new ChatUserEventMessage
        // {
        //     Message = answer,
        //     FromUser = false,
        //     UserId = userId
        // });
    }
    public async Task<(string completionText, int completionTokens)> GetChatCompletionAsync(string sessionId, string userPrompt, string userId)
    {
        await _adminHubManager.ChatUserEvent(userId, new ChatUserEventMessage
        {
            Message = userPrompt,
            FromUser = true,
            UserId = userId
        });

        ChatMessage systemMessage = new(ChatRole.System, _systemPrompt);
        ChatMessage userMessage = new(ChatRole.User, userPrompt);

        ChatCompletionsOptions options = new()
        {

            Messages =
            {
                systemMessage,
                userMessage
            },
            User = sessionId,
            MaxTokens = 2000,
            Temperature = 0.3f,
            NucleusSamplingFactor = 0.5f,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        Azure.Response<ChatCompletions> completionsResponse = await _client.GetChatCompletionsAsync(_modelName, options);

        var completions = completionsResponse.Value;
        var answer = completions.Choices[0].Message.Content;

        await _adminHubManager.ChatEvent(new ChatUserEvent
        {
            UserId = userId
        });
        await _adminHubManager.ChatUserEvent(userId, new ChatUserEventMessage
        {
            Message = answer,
            FromUser = false,
            UserId = userId
        });

        return (
            completionText: answer,
            completionTokens: completions.Usage.CompletionTokens
        );
    }

    /// <summary>
    /// Sends the existing conversation to the OpenAI model and returns a two word summary.
    /// </summary>
    /// <param name="sessionId">Chat session identifier for the current conversation.</param>
    /// <param name="conversationText">conversation history to send to OpenAI.</param>
    /// <returns>Summarize response from the OpenAI model deployment.</returns>
    public async Task<string> SummarizeAsync(string sessionId, string conversationText)
    {

        ChatMessage systemMessage = new(ChatRole.System, _summarizePrompt);
        ChatMessage userMessage = new(ChatRole.User, conversationText);

        ChatCompletionsOptions options = new()
        {
            Messages = {
                systemMessage,
                userMessage
            },
            User = sessionId,
            MaxTokens = 200,
            Temperature = 0.0f,
            NucleusSamplingFactor = 1.0f,
            FrequencyPenalty = 0,
            PresencePenalty = 0
        };

        Azure.Response<ChatCompletions> completionsResponse = await _client.GetChatCompletionsAsync(_modelName, options);

        var completions = completionsResponse.Value;

        var completionText = completions.Choices[0].Message.Content;

        return completionText;
    }
}