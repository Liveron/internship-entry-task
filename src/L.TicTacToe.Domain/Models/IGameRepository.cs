namespace L.TicTacToe.Domain.Models;

public interface IGameRepository
{
    Task<Game> GetByIdAsync(Guid gameId);
    Task<(Guid gameId, long version)> CreateGameAsync(Game game);
    Task<long> UpdateGameAsync(Game game, long actualVersion);
    Task<long> GetGameVersionAsync(Guid gameId);
}
