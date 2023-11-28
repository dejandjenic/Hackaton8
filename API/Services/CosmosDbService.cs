using API.Configuration;
using API.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

namespace API.Services
{
	public interface ICosmosDbService
	{
		Task<ChatHistory> InsertChatHistoryAsync(ChatHistory chatHistory);
		Task<List<ChatHistory>> GetChatHistoryItemsAsync(string userId);
	}
	public class CosmosDbService : ICosmosDbService
	{
		private readonly Database _database;
		private readonly string _cosmosDatabase = "hackaton";

		public CosmosDbService(AppSettings appSettings)
		{
			CosmosSerializationOptions options = new()
			{
				PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
			};

			var client = new CosmosClientBuilder(appSettings.CosmosEndpoint, appSettings.CosmosKey)
				.WithSerializerOptions(options)
				.Build();

			_database = client.GetDatabase(_cosmosDatabase);

		}

		/// <summary>
		/// Create container for chat history if not exist
		/// </summary>
		/// <returns>Newly created container or existing for specific user</returns>
		private async Task<Container> GetContainer()
		{
			return await _database.CreateContainerIfNotExistsAsync(id: "chatHistory", partitionKeyPath: "/id");
		}

		/// <summary>
		/// Creates a new chat history item.
		/// </summary>
		/// <param name="chatHistory">Chat history item to create.</param>
		/// <returns>Newly created chat history item.</returns>
		public async Task<ChatHistory> InsertChatHistoryAsync(ChatHistory chatHistory)
		{
			var container = await GetContainer();
			PartitionKey partitionKey = new(chatHistory.Id);
			return await container.CreateItemAsync<ChatHistory>(
				item: chatHistory,
				partitionKey: partitionKey
			);
		}

		/// <summary>
		/// Gets a list of all current chat history items.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns>List of distinct chat history items.</returns>
		public async Task<List<ChatHistory>> GetChatHistoryItemsAsync(string userId)
		{
			var container = await GetContainer();
			QueryDefinition query = new QueryDefinition("SELECT DISTINCT * FROM c WHERE c.type = @type AND c.userId = @userId")
				.WithParameter("@type", nameof(ChatHistory))
				.WithParameter("@userId", userId);

			FeedIterator<ChatHistory> response = container.GetItemQueryIterator<ChatHistory>(query);

			List<ChatHistory> output = new();
			while (response.HasMoreResults)
			{
				FeedResponse<ChatHistory> results = await response.ReadNextAsync();
				output.AddRange(results);
			}
			return output;
		}
	}
}
