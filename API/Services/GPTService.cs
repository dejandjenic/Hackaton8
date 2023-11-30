using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using API.Configuration;
using API.Hubs;
using API.Repositories;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.Memory.AzureCognitiveSearch;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;
using Shared;

namespace API.Services;

public interface IGPTService
{
	Task Respond(string userId, string message);
	Task RespondAsAdmin(string userId, string message);
}

public class GPTService : IGPTService
{
	private readonly IChatService chatService;
	private readonly AppSettings appSettings;
	private readonly IUserHubManager userHubManager;
	private readonly IAdminHubManager adminHubManager;
	private readonly ICosmosDbService cosmosDbService;

	private readonly OpenAIClient aoai;
	private readonly string aoaiModel;


	public GPTService(IChatService chatService, AppSettings appSettings, IUserHubManager userHubManager, IAdminHubManager adminHubManager, ICosmosDbService cosmosDbService)
	{
		this.chatService = chatService;
		this.appSettings = appSettings;
		this.userHubManager = userHubManager;
		this.adminHubManager = adminHubManager;
		this.cosmosDbService = cosmosDbService;

		string aoaiEndpoint = appSettings.OpenAIEndpoint;
		string aoaiApiKey = appSettings.OpenAIKey;
		aoaiModel = appSettings.OpenAIModel;
		aoai = new OpenAIClient(new Uri(aoaiEndpoint), new AzureKeyCredential(aoaiApiKey));

	}
	public async Task Respond(string userId, string message)
	{
		await adminHubManager.ChatUserEvent(userId, new ChatUserEventMessage
		{
			Text = message,
			FromUser = true,
			UserId = userId,
		});

		var history = await chatService.GetHistory(userId);
		var activeUser = await cosmosDbService.GetActiveUser(userId);

		var useRealChat = !activeUser.ChatPaused;

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

		string acsEndpoint = appSettings.SearchEndpoint;
		string acsApiKey = appSettings.SearchKey;
		string collectionName = appSettings.SearchCollectionName;

		var answer = "";

		StringBuilder builder = new();
		if (useRealChat)
		{

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

			var question = message;

			chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, question));
			await chatService.SaveNewChatItem(userId, question, true, ChatRole.User.ToString());
			await SummarizeAfterFiveItems(userId);

			builder.Clear();
			var response = await aoai.GetChatCompletionsStreamingAsync(aoaiModel, chatCompletionsOptions);

			await foreach (StreamingChatChoice choice in response.Value.GetChoicesStreaming())
			{
				await foreach (ChatMessage item in choice.GetMessageStreaming())
				{
					builder.Append(item.Content);
				}
			}

			answer = builder.ToString();
			chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.Assistant, answer));
		}
		else
		{
			answer = "...";
		}


		await chatService.SaveNewChatItem(userId, answer, false, ChatRole.Assistant.ToString());

		await userHubManager.Respond(userId, answer);

		if (await SummarizeAfterFiveItems(userId))
			activeUser = await cosmosDbService.GetActiveUser(userId); //if chat name updated, get updated user

		await adminHubManager.ChatEvent(new ChatUserEvent
		{
			UserId = userId,
			Name = activeUser.ChatName,
			IsPaused = activeUser.ChatPaused
		});
		await adminHubManager.ChatUserEvent(userId, new ChatUserEventMessage
		{
			Text = answer,
			FromUser = false,
			UserId = userId,
		});
	}

	public async Task RespondAsAdmin(string userId, string answer)
	{
		var activeUser = await cosmosDbService.GetActiveUser(userId);
		await chatService.SaveNewChatItem(userId, answer, false, ChatRole.System.ToString());

		await userHubManager.Respond(userId, answer);

		if (await SummarizeAfterFiveItems(userId))
			activeUser = await cosmosDbService.GetActiveUser(userId); //if chat name updated, get updated user

		await adminHubManager.ChatEvent(new ChatUserEvent
		{
			UserId = userId,
			Name = activeUser.ChatName,
			IsPaused = activeUser.ChatPaused
		});

		await adminHubManager.ChatUserEvent(userId, new ChatUserEventMessage
		{
			Text = answer,
			FromUser = false,
			UserId = userId,

		});

	}


	public async Task SummarizeAsync(string userId, string conversationText)
	{
		var _summarizePrompt = @" Summarize this prompt in one or two words. Do not use any punctuation." + Environment.NewLine;
		ChatMessage systemMessage = new(ChatRole.System, _summarizePrompt);
		ChatMessage userMessage = new(ChatRole.User, conversationText);

		ChatCompletionsOptions options = new()
		{
			Messages = {
				systemMessage,
				userMessage
			},
			MaxTokens = 200,
			Temperature = 0.0f,
			NucleusSamplingFactor = 1.0f,
			FrequencyPenalty = 0,
			PresencePenalty = 0
		};

		Response<ChatCompletions> completionsResponse = await aoai.GetChatCompletionsAsync(aoaiModel, options);

		ChatCompletions completions = completionsResponse.Value;

		string completionText = completions.Choices[0].Message.Content;

		await cosmosDbService.UpdateUserChatName(userId, completionText);
	}

	private async Task<bool> SummarizeAfterFiveItems(string userId)
	{
		var history = await chatService.GetHistory(userId);
		if (history.Count != 0 && history.Count % 5 == 0)
		{
			var textLastFiveMessages = history.TakeLast(5).Select(h => h.Text);
			await SummarizeAsync(userId, string.Join(Environment.NewLine, textLastFiveMessages));
			return true;
		}

		return false;
	}
}