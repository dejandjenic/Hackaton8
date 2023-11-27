using API.Models.Cosmos;
using API.Repositories;
using Shared.Constants;
using Shared;
using SharpToken;

namespace API.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly IGPTService _gptService;

    /// <summary>
    /// All data is cached in the _sessions List object.
    /// </summary>
    private static List<Session> _sessions = new();
    private readonly int _maxConversationTokens = 4000;

    public ChatService(IChatRepository chatRepository, ICosmosDbService cosmosDbService, IGPTService gptService)
    {
        _chatRepository = chatRepository;
        _cosmosDbService = cosmosDbService;
        _gptService = gptService;
    }
    public async Task<List<ChatHistory>> GetHistory(string userId)
    {
        return await _chatRepository.GetHistory(userId);
    }

    public async Task SaveNewChatItem(string userId, string text, bool fromUser)
    {
        await _chatRepository.SaveNewChatItem(userId, text, fromUser);
    }

    /// <summary>
    /// Returns list of chat session ids and names for left-hand nav to bind to (display Name and ChatSessionId as hidden)
    /// </summary>
    public async Task<List<Session>> GetAllChatSessionsAsync(string userId)
    {
        return _sessions = await _cosmosDbService.GetSessionsAsync(userId);
    }

    /// <summary>
    /// Returns the chat messages to display on the main web page when the user selects a chat from the left-hand nav
    /// </summary>
    public async Task<List<Message>> GetChatSessionMessagesAsync(string? sessionId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        List<Message> chatMessages = new();

        if (_sessions.Count == 0)
        {
            return Enumerable.Empty<Message>().ToList();
        }

        int index = _sessions.FindIndex(s => s.SessionId == sessionId);

        if (_sessions[index].Messages.Count == 0)
        {
            // Messages are not cached, go read from database
            chatMessages = await _cosmosDbService.GetSessionMessagesAsync(sessionId);

            // Cache results
            _sessions[index].Messages = chatMessages;
        }
        else
        {
            // Load from cache
            chatMessages = _sessions[index].Messages;
        }

        return chatMessages;
    }

    /// <summary>
    /// User creates a new Chat Session.
    /// </summary>
    public async Task<Session> CreateNewChatSessionAsync(string userId)
    {
        Session session = new(userId);

        _sessions.Add(session);

        await _cosmosDbService.InsertSessionAsync(session);

        return session;
    }

    /// <summary>
    /// Get a completion from _openAiService
    /// </summary>
    public async Task<string> GetChatCompletionAsync(string? sessionId, string promptText, string userId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        //Create a message object for the User Prompt and calculate token usage
        Message prompt = CreatePromptMessage(sessionId, promptText);

        //Grab conversation history up to the maximum configured tokens
        string conversation = GetChatSessionConversation(sessionId);

        //Generate a completion and tokens used from the user prompt and conversation
        (string completionText, int completionTokens) = await _gptService.GetChatCompletionAsync(sessionId, conversation, userId);

        //Create a message object for the completion
        Message completion = CreateCompletionMessage(sessionId, completionTokens, completionText);

        //Update the tokens used in the session
        Session session = UpdateSessionTokens(sessionId, prompt.Tokens, completion.Tokens);

        //Insert/Update all of it in a transaction to Cosmos
        await _cosmosDbService.UpsertSessionBatchAsync(prompt, completion, session);

        return completionText;
    }

    /// <summary>
    /// Get current conversation, including latest user prompt, from newest to oldest up to max conversation tokens
    /// </summary>
    private string GetChatSessionConversation(string sessionId)
    {

        int? tokensUsed = 0;

        List<string> conversationBuilder = new List<string>();

        int index = _sessions.FindIndex(s => s.SessionId == sessionId);

        List<Message> messages = _sessions[index].Messages;


        //Start at the end of the list and work backwards
        //This includes the latest user prompt which is already cached
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            tokensUsed += messages[i].Tokens is null ? 0 : messages[i].Tokens;

            if (tokensUsed > _maxConversationTokens)
                break;

            conversationBuilder.Add(messages[i].Text);
        }

        //Invert the chat messages to put back into chronological order and output as string.        
        string conversation = string.Join(Environment.NewLine, conversationBuilder.Reverse<string>());

        return conversation;

    }

    /// <summary>
    /// Have OpenAI summarize the conversation based upon the prompt and completion text in the session
    /// </summary>
    public async Task<string> SummarizeChatSessionNameAsync(string? sessionId, string conversationText)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        string completionText = await _gptService.SummarizeAsync(sessionId, conversationText);

        await RenameChatSessionAsync(sessionId, completionText);

        return completionText;
    }


    /// <summary>
    /// Calculate token count for prompt text. Add user prompt to the chat session message list object
    /// </summary>
    private Message CreatePromptMessage(string sessionId, string promptText)
    {
        Message promptMessage = new(sessionId, nameof(Participants.User), default, promptText);

        //Calculate tokens for the user prompt message. OpenAI calculates tokens for completion so can get those from there 
        promptMessage.Tokens = GetTokens(promptText);

        //Add to the cache
        int index = _sessions.FindIndex(s => s.SessionId == sessionId);
        _sessions[index].AddMessage(promptMessage);

        return promptMessage;

    }

    /// <summary>
    /// Add completion to the chat session message list object
    /// </summary>
    private Message CreateCompletionMessage(string sessionId, int completionTokens, string completionText)
    {
        //Create completion message
        Message completionMessage = new(sessionId, nameof(Participants.Assistant), completionTokens, completionText);

        //Add to the cache
        int index = _sessions.FindIndex(s => s.SessionId == sessionId);
        _sessions[index].AddMessage(completionMessage);

        return completionMessage;
    }

    /// <summary>
    /// Update session with user prompt and completion tokens and update the cache
    /// </summary>
    private Session UpdateSessionTokens(string sessionId, int? promptTokens, int? completionTokens)
    {

        int index = _sessions.FindIndex(s => s.SessionId == sessionId);

        //Update session with user prompt and completion tokens and update the cache
        _sessions[index].TokensUsed += promptTokens;
        _sessions[index].TokensUsed += completionTokens;

        return _sessions[index];

    }

    /// <summary>
    /// Calculate the number of tokens from the user prompt
    /// </summary>
    private int GetTokens(string userPrompt)
    {
        //Create a new instance of SharpToken
        var encoding = GptEncoding.GetEncodingForModel("gpt-3.5-turbo");

        //Get count of vectors on user prompt (return)
        return encoding.Encode(userPrompt).Count;

    }

    /// <summary>
    /// Rename the Chat Session from "New Chat" to the summary provided by OpenAI
    /// </summary>
    private async Task RenameChatSessionAsync(string? sessionId, string newChatSessionName)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        int index = _sessions.FindIndex(s => s.SessionId == sessionId);

        _sessions[index].Name = newChatSessionName;

        await _cosmosDbService.UpdateSessionAsync(_sessions[index]);
    }
}