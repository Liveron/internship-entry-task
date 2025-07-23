using L.TicTacToe.Domain.Models;

namespace L.TicTacToe.Infrastructure.Repositories;

public class GameRepository(IEventStore eventStore) : IGameRepository
{
    private readonly IEventStore _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));

    public async Task<(Guid gameId, long version)> CreateGameAsync(Game game)
    {
        var version = await _eventStore.SaveEventsAsync(game.DomainEvents, game.Id);
        return (game.Id, version);
    }

    public async Task<long> UpdateGameAsync(Game game, long actualVersion)
    {
         return await _eventStore.SaveEventsAsync(game.DomainEvents, game.Id, actualVersion);
    }

    public async Task<long> GetGameVersionAsync(Guid gameId)
    {
        return await _eventStore.GetEventStreamVersionAsync(gameId);
    }

    public async Task<Game> GetByIdAsync(Guid gameId)
    {
        var events = await _eventStore.GetEventsAsync(gameId);
        return new Game(events);
    }
}
