using API.Models.Cosmos;
using API.Repositories;
using Shared;

namespace API.Services;

public interface IChatService
{
    Task<List<ChatHistory>> GetHistory(string userId);
    Task SaveNewChatItem(string userId, string text, bool fromUser);
    Task<List<Session>> GetAllChatSessionsAsync(string userId);
    Task<List<Message>> GetChatSessionMessagesAsync(string? sessionId);
    Task<Session> CreateNewChatSessionAsync(string userId);
    Task<string> GetChatCompletionAsync(string? sessionId, string promptText, string userId);
    Task<string> SummarizeChatSessionNameAsync(string? sessionId, string conversationText);
}