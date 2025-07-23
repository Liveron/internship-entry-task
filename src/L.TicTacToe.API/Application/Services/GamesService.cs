using L.TicTacToe.API.Options;
using L.TicTacToe.Domain.Exceptions;
using L.TicTacToe.Domain.Models;
using L.TicTacToe.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;

namespace L.TicTacToe.API.Application.Services;

public sealed class GamesService(IHttpContextAccessor httpContextAccessor, IGameRepository gameRepository,
    IOptions<GameOptions> options) : IGamesService
{
    private readonly HttpContext _httpContext = httpContextAccessor?.HttpContext 
        ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    private readonly GameOptions gameOptions = options?.Value 
        ?? throw new ArgumentNullException(nameof(options));

    private readonly IGameRepository _gameRepository = gameRepository
        ?? throw new ArgumentNullException(nameof(gameRepository));

    public async Task<Guid> CreateGameAsync(Guid firstPlayer, Guid secondPlayer)
    {
        var game = new Game(firstPlayer, secondPlayer, gameOptions.TableSize, gameOptions.WinLength);
        var (gameId, version) = await _gameRepository.CreateGameAsync(game);

        _httpContext.SetResponseETagVersionHeader(version);

        return gameId;
    }

    public async Task<Game> GetGameByIdAsync(Guid playerId, Guid gameId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);

        if (playerId != game.FirstPlayerId && playerId != game.SecondPlayerId)
            throw new DomainException("Вы не имеете доступа к этой игре.");

        _httpContext.SetResponseETagVersionHeader(game.Version);

        return game;
    }

    public async Task MakeMoveAsync(Guid gameId, Guid playerId, int row, int column)
    {
        var expectedVersion = _httpContext.GetRequestIfMatchVersionHeader();
        var actualVersion = await _gameRepository.GetGameVersionAsync(gameId);

        if (expectedVersion != actualVersion)
            throw new ConcurrencyException(expectedVersion, actualVersion);

        var game = await _gameRepository.GetByIdAsync(gameId);
        game.MakeMove(playerId, row, column);
        var version = await _gameRepository.UpdateGameAsync(game, actualVersion);

        _httpContext.SetResponseETagVersionHeader(version);
    }
}
