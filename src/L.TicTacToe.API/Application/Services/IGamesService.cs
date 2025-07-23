using L.TicTacToe.Domain.Models;

namespace L.TicTacToe.API.Application.Services;

public interface IGamesService
{
    public Task<Game> GetGameByIdAsync(Guid playerId, Guid gameId);
    public Task<Guid> CreateGameAsync(Guid firstPlayer, Guid secondPlayer);
    public Task MakeMoveAsync(Guid gameId, Guid playerId, int row, int column);
}
