using API.Configuration;
using API.Models.Cosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Shared;

namespace API.Services
{
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
        /// Create container for specific user if not exist
        /// </summary>
        /// <returns>Newly created container or existing for specific user</returns>
        private async Task<Container> GetContainer()
        {
            return await _database.CreateContainerIfNotExistsAsync(id: "sessions", partitionKeyPath: "/sessionId");
        }

        /// <summary>
        /// Creates a new chat session.
        /// </summary>
        /// <param name="session">Chat session item to create.</param>
        /// <returns>Newly created chat session item.</returns>
        public async Task<Session> InsertSessionAsync(Session session)
        {
            var container = await GetContainer();
            PartitionKey partitionKey = new(session.SessionId);
            return await container.CreateItemAsync<Session>(
                item: session,
                partitionKey: partitionKey
            );
        }


        /// <summary>
        /// Gets a list of all current chat sessions.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>List of distinct chat session items.</returns>
        public async Task<List<Session>> GetSessionsAsync(string userId)
        {
            var container = await GetContainer();
            QueryDefinition query = new QueryDefinition("SELECT DISTINCT * FROM c WHERE c.type = @type AND c.userId = @userId")
                .WithParameter("@type", nameof(Session))
                .WithParameter("@userId", userId);

            FeedIterator<Session> response = container.GetItemQueryIterator<Session>(query);

            List<Session> output = new();
            while (response.HasMoreResults)
            {
                FeedResponse<Session> results = await response.ReadNextAsync();
                output.AddRange(results);
            }
            return output;
        }


        /// <summary>
        /// Gets a list of all current chat messages for a specified session identifier.
        /// </summary>
        /// <param name="sessionId">Chat session identifier used to filter messsages.</param>
        /// <returns>List of chat message items for the specified session.</returns>
        public async Task<List<Message>> GetSessionMessagesAsync(string sessionId)
        {
            var container = await GetContainer();
            QueryDefinition query = new QueryDefinition("SELECT * FROM c WHERE c.sessionId = @sessionId AND c.type = @type")
                .WithParameter("@sessionId", sessionId)
                .WithParameter("@type", nameof(Message));

            FeedIterator<Message> results = container.GetItemQueryIterator<Message>(query);

            List<Message> output = new();
            while (results.HasMoreResults)
            {
                FeedResponse<Message> response = await results.ReadNextAsync();
                output.AddRange(response);
            }
            return output;
        }

        /// <summary>
        /// Batch create or update chat messages and session.
        /// </summary>
        /// <param name="messages">Chat message and session items to create or replace.</param>
        public async Task UpsertSessionBatchAsync(params dynamic[] messages)
        {
            var container = await GetContainer();

            if (messages.Select(m => m.SessionId).Distinct().Count() > 1)
            {
                throw new ArgumentException("All items must have the same partition key.");
            }

            PartitionKey partitionKey = new(messages.First().SessionId);
            TransactionalBatch batch = container.CreateTransactionalBatch(partitionKey);
            foreach (var message in messages)
            {
                batch.UpsertItem(
                    item: message
                );
            }
            await batch.ExecuteAsync();
        }

        /// <summary>
        /// Updates an existing chat session.
        /// </summary>
        /// <param name="session">Chat session item to update.</param>
        /// <returns>Revised created chat session item.</returns>
        public async Task<Session> UpdateSessionAsync(Session session)
        {
            var container = await GetContainer();

            PartitionKey partitionKey = new(session.SessionId);
            return await container.ReplaceItemAsync(
                item: session,
                id: session.Id,
                partitionKey: partitionKey
            );
        }

    }
}

