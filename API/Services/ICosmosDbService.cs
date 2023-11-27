using API.Models.Cosmos;
using Shared;

namespace API.Services
{
    public interface ICosmosDbService
    {
        Task<Session> InsertSessionAsync(Session session);
        Task<List<Session>> GetSessionsAsync(string userId);
        Task<List<Message>> GetSessionMessagesAsync(string sessionId);
        Task UpsertSessionBatchAsync(params dynamic[] messages);
        Task<Session> UpdateSessionAsync(Session session);
    }
}
