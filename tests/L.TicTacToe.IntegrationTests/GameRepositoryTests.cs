using L.TicTacToe.Domain.Models;
using L.TicTacToe.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace L.TicTacToe.IntegrationTests;

[Collection("TicTacToe Test Collection")]
public sealed class GameRepositoryTests(TicTacToeFixture fixture) : IDisposable
{
    private readonly Guid _firstPlayerId = Guid.NewGuid();
    private readonly Guid _secondPlayerId = Guid.NewGuid();

    private readonly EventsContext _context = fixture.EventsContext;
    private readonly IGameRepository _gameRepository = fixture.GameRepository;

    [Fact]
    public async Task CreateGameAsync_ShouldSaveGame()
    {
        // Arrange
        var game = new Game(_firstPlayerId, _secondPlayerId, 3, 3);

        // Act
        var (createdGameId, _) = await _gameRepository.CreateGameAsync(game);
        var createdGame = await _gameRepository.GetByIdAsync(createdGameId);

        // Assert
        Assert.Equal(createdGameId, createdGame.Id);
        Assert.Equal(_firstPlayerId, createdGame.FirstPlayerId);
        Assert.Equal(_secondPlayerId, createdGame.SecondPlayerId);
        Assert.Equal(_firstPlayerId, createdGame.CurrentPlayer);
        Assert.Equal(1, createdGame.CurrentMove);
    }

    [Fact]
    public async Task UpdateGameAsync_ShouldSaveUpdatedGame()
    {
        // Arrange
        var game = new Game(_firstPlayerId, _secondPlayerId, 3, 3);

        // Act
        var (createdGameId, _) = await _gameRepository.CreateGameAsync(game);
        var createdGame = await _gameRepository.GetByIdAsync(createdGameId);
        createdGame.MakeMove(_firstPlayerId, 0, 0);
        createdGame.MakeMove(_secondPlayerId, 0, 1);
        await _gameRepository.UpdateGameAsync(createdGame, 1);
        var updatedGame = await _gameRepository.GetByIdAsync(createdGameId);

        // Assert
        Assert.Equal(createdGameId, updatedGame.Id);
        Assert.Equal(_firstPlayerId, updatedGame.FirstPlayerId);
        Assert.Equal(_secondPlayerId, updatedGame.SecondPlayerId);
        Assert.Equal(3, updatedGame.CurrentMove);
    }

    public void Dispose()
    {
        _context.EventEntries.ExecuteDelete();
    }
}
