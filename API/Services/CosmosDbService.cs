using API.Configuration;
using API.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Shared;

namespace API.Services
{
	public interface ICosmosDbService
	{
		Task SaveSettings(ChatSettings content);
		Task<ChatSettings> GetSettings();
		Task CreateUser(string id);
		Task UpdateUserLastInteraction(string id);
		Task UpdateUserActive(string id, bool active);
		Task UpdateUserChatPaused(string id, bool? paused);
		Task<List<ChatUser>> GetActiveUsers();
		Task<KnowledgeBasePage> GetPage(string id);
		Task DeletePage(string id);
		Task AddPage(string id, string text, string content);
		Task UpdatePage(string id, string text, string content, int lines);
		Task<List<KnowledgeBasePage>> GetPages();
		Task<ChatHistory> InsertChatHistoryAsync(ChatHistory chatHistory);
		Task<List<ChatHistory>> GetChatHistoryItemsAsync(string userId);
		Task<ChatUser> GetActiveUser(string id);
		Task UpdateUserChatName(string id, string name);
	}
	public class CosmosDbService : ICosmosDbService
	{
		private readonly Database _database;
		private readonly string _cosmosDatabase = "hackaton";
		private const string settingsId = "SETTINGS";

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

		private async Task<Container> GetContainerForSettings()
		{
			return await _database.CreateContainerIfNotExistsAsync(id: "pages", partitionKeyPath: "/id");
		}

		public async Task DeletePage(string id)
		{
			var container = await GetContainer();
			PartitionKey partitionKey = new(id);
			await container.DeleteItemAsync<KnowledgeBasePage>(id, partitionKey
			);
		}

		public async Task AddPage(string id, string text, string content)
		{
			var container = await GetContainer();
			PartitionKey partitionKey = new(id);
			await container.CreateItemAsync(
				item: new KnowledgeBasePage(text, id, content),
				partitionKey: partitionKey
			);
		}

		public async Task UpdatePage(string id, string text, string content, int lines)
		{
			var container = await GetContainer();
			PartitionKey partitionKey = new(id);
			await container.UpsertItemAsync(
				item: new KnowledgeBasePage(text, id, content, lines),
				partitionKey: partitionKey
			);
		}

		public async Task<List<KnowledgeBasePage>> GetPages()
		{
			var container = await GetContainer();
			QueryDefinition query = new QueryDefinition("SELECT DISTINCT * FROM c WHERE c.type = @type")
				.WithParameter("@type", nameof(KnowledgeBasePage))
				;

			FeedIterator<KnowledgeBasePage> response = container.GetItemQueryIterator<KnowledgeBasePage>(query);

			List<KnowledgeBasePage> output = new();
			while (response.HasMoreResults)
			{
				FeedResponse<KnowledgeBasePage> results = await response.ReadNextAsync();
				output.AddRange(results);
			}
			return output;
		}

		public async Task<List<ChatUser>> GetActiveUsers()
		{
			var container = await GetContainer();
			QueryDefinition query = new QueryDefinition("SELECT DISTINCT * FROM c WHERE c.type = @type AND c.Active = @active")
					.WithParameter("@type", nameof(ChatUser))
					.WithParameter("@active", true)
				;

			FeedIterator<ChatUser> response = container.GetItemQueryIterator<ChatUser>(query);

			List<ChatUser> output = new();
			while (response.HasMoreResults)
			{
				FeedResponse<ChatUser> results = await response.ReadNextAsync();
				output.AddRange(results);
			}
			return output;
		}

		public async Task SaveSettings(ChatSettings content)
		{
			try
			{
				var container = await GetContainer();
				PartitionKey partitionKey = new(settingsId);
				await container.PatchItemAsync<ChatSettings>(settingsId, partitionKey, new List<PatchOperation>()
				{
					PatchOperation.Replace("/text", content.Text),
				});
			}
			catch (Microsoft.Azure.Cosmos.CosmosException ex)
			{
				var container = await GetContainer();
				PartitionKey partitionKey = new(settingsId);
				await container.CreateItemAsync(content, partitionKey);
			}
		}

		public async Task<ChatSettings> GetSettings()
		{
			try
			{
				PartitionKey partitionKey = new(settingsId);
				var container = await GetContainer();
				return await container.ReadItemAsync<ChatSettings>(settingsId, partitionKey);
			}
			catch
			{
				return null;
			}
		}

		public async Task CreateUser(string id)
		{
			var container = await GetContainer();
			PartitionKey partitionKey = new(id);
			await container.CreateItemAsync(
				item: new ChatUser()
				{
					Id = id,
					LastInteraction = DateTime.UtcNow,
					FirstInteraction = DateTime.UtcNow
				},
				partitionKey: partitionKey
			);
		}
		public async Task<ChatUser> GetActiveUser(string id)
		{
			var container = await GetContainer();
			QueryDefinition query = new QueryDefinition("SELECT DISTINCT * FROM c WHERE c.type = @type AND c.id = @id")
				.WithParameter("@type", nameof(ChatUser))
				.WithParameter("@id", id);

			FeedIterator<ChatUser> response = container.GetItemQueryIterator<ChatUser>(query);

			List<ChatUser> output = new();
			while (response.HasMoreResults)
			{
				FeedResponse<ChatUser> results = await response.ReadNextAsync();
				output.AddRange(results);
			}
			return output.First();
		}

		public async Task UpdateUserLastInteraction(string id)
		{
			var container = await GetContainer();
			PartitionKey partitionKey = new(id);
			await container.PatchItemAsync<ChatUser>(id, partitionKey, new List<PatchOperation>()
			{
				PatchOperation.Replace("/lastInteraction", DateTime.UtcNow),
			});
		}

		public async Task UpdateUserActive(string id, bool active)
		{
			var container = await GetContainer();
			PartitionKey partitionKey = new(id);
			await container.PatchItemAsync<ChatUser>(id, partitionKey, new List<PatchOperation>()
			{
				PatchOperation.Replace("/active", active),
			});
		}

		public async Task UpdateUserChatPaused(string id, bool? paused)
		{
			var container = await GetContainer();
			PartitionKey partitionKey = new(id);
			await container.PatchItemAsync<ChatUser>(id, partitionKey, new List<PatchOperation>()
			{
				PatchOperation.Replace("/chatPaused", paused),
			});
		}

		public async Task UpdateUserChatName(string id, string name)
		{
			var container = await GetContainer();
			PartitionKey partitionKey = new(id);
			await container.PatchItemAsync<ChatUser>(id, partitionKey, new List<PatchOperation>()
			{
				PatchOperation.Replace("/chatName", name),
			});
		}

		public async Task<KnowledgeBasePage> GetPage(string id)
		{
			var container = await GetContainer();
			PartitionKey partitionKey = new(id);
			KnowledgeBasePage readItem = await container.ReadItemAsync<KnowledgeBasePage>(
				id: id,
				partitionKey: partitionKey
			);

			return readItem;
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
