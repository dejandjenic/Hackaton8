using API.Services;

namespace API.Repositories;

public class ChatRepository : IChatRepository
{
	private readonly ICosmosDbService _cosmosDb;

	public ChatRepository(ICosmosDbService cosmosDb)
	{
		_cosmosDb = cosmosDb;
	}

	public async Task<List<ChatHistory>> GetHistory(string userId)
	{
		return await _cosmosDb.GetChatHistoryItemsAsync(userId);
	}

	public async Task SaveNewChatItem(string userId, string text, bool fromUser, string role)
	{
		var historyItem = new ChatHistory(userId, fromUser, text, DateTime.UtcNow, role);
		await _cosmosDb.InsertChatHistoryAsync(historyItem);
	}
}