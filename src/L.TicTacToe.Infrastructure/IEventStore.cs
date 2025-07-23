using L.TicTacToe.Domain.Setup;

namespace L.TicTacToe.Infrastructure;

public interface IEventStore
{
    Task<long> SaveEventsAsync(IEnumerable<IDomainEvent> events, Guid modelId, long actualVersion = 0);
    Task<List<IDomainEvent>> GetEventsAsync(Guid modelId);
    Task<long> GetEventStreamVersionAsync(Guid modelId);
}