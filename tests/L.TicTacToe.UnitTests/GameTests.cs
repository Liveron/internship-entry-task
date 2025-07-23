using L.TicTacToe.Domain.Events;
using L.TicTacToe.Domain.Exceptions;
using L.TicTacToe.Domain.Models;
using Moq;

namespace L.TicTacToe.UnitTests;

public sealed class GameTests
{
    private readonly Guid _gameId = Guid.NewGuid();
    private readonly Guid _player1Id = Guid.NewGuid();
    private readonly Guid _player2Id = Guid.NewGuid();
    private const int BoardSize = 3;
    private const int WinLength = 3;

    private Game CreateDefautlGame()
    {
        var mockRandom = new Mock<Random>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.2);
        return new(_player1Id, _player2Id, BoardSize, WinLength, mockRandom.Object);
    }

    [Fact]
    public void Constructor_AddsGameStartedEvent()
    {
        // Arrange & Act
        var game = CreateDefautlGame();

        // Assert
        var @event = Assert.Single(game.DomainEvents);
        var gameStartedEvent = Assert.IsType<GameStartedDomainEvent>(@event);
        Assert.Equal(_player1Id, gameStartedEvent.FirstPlayerId);
        Assert.Equal(_player2Id, gameStartedEvent.SecondPlayerId);
        Assert.Equal(WinLength, gameStartedEvent.WinLength);
    }

    [Fact]
    public void Constructor_InitializesGame()
    {
        // Arrange & Act
        var game = CreateDefautlGame();

        // Assert
        Assert.Equal(_player1Id, game.FirstPlayerId);
        Assert.Equal(_player2Id, game.SecondPlayerId);
        Assert.Equal(_player1Id, game.CurrentPlayer);
        Assert.Equal(1, game.CurrentMove);
        Assert.Equal(WinLength, game.WinLength);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Theory]
    [InlineData(3, 0)]
    [InlineData(3, 4)]
    public void Constructor_CorrectlyInitializesWinLength_WithInvalidWinLength(int size, int winLength)
    {
        // Arrange & Act
        var game = new Game(_player1Id, _player2Id, size, winLength);

        // Assert
        Assert.Equal(size, game.WinLength);
    }

    [Fact]
    public void MakeMove_ThrowsException_WhenIncorrectMove()
    {
        // Arrange
        var game = CreateDefautlGame();

        // Act & Assert
        Assert.Throws<DomainException>(() => game.MakeMove(_player2Id, 0, 0));
    }

    [Fact]
    public void MakeMove_ReplacesMark_WhenCheatMoveOccur()
    {
        // Arrange
        var mockRandom = new Mock<Random>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.09);
        var game = new Game(_player1Id, _player2Id, BoardSize, WinLength, mockRandom.Object);

        // Act
        game.MakeMove(_player1Id, 0, 0);
        game.MakeMove(_player2Id, 0, 1);
        game.MakeMove(_player1Id, 1, 1);
        game.MakeMove(_player2Id, 2, 1);

        // Assert
        Assert.Contains(game.DomainEvents, e => e is CheatMoveMaidDomainEvent);
        var @event = game.DomainEvents.Last();
        var gameFinishedEvent = Assert.IsType<GameFinishedDomainEvent>(@event);
        Assert.Equal(_player2Id, gameFinishedEvent.Winner);
    }

    [Fact]
    public void MakeMove_Makes10PercentCheatMove_OnThirdMove()
    {
        // Arrange
        var trials = 10000;
        int cheatMoves = 0;

        // Act
        for (int i = 0; i < trials; i++)
        {
            var game = new Game(_player1Id, _player2Id, BoardSize, WinLength);
            game.MakeMove(_player1Id, 0, 0);
            game.MakeMove(_player2Id, 0, 1);
            game.MakeMove(_player1Id, 0, 2);
            if (game.DomainEvents.Last() is CheatMoveMaidDomainEvent)
                cheatMoves++;
        }

        double successRate = (double)cheatMoves / trials;

        // Assert
        Assert.InRange(successRate, 0.08, 0.12);
    }

    [Fact]
    public void MakeMove_FinishesGame_AfterWin()
    {
        // Arrange
        var game = CreateDefautlGame();

        // Act
        game.MakeMove(_player1Id, 0, 0);
        game.MakeMove(_player2Id, 1, 0);
        game.MakeMove(_player1Id, 0, 1);
        game.MakeMove(_player2Id, 1, 1);
        game.MakeMove(_player1Id, 0, 2);

        // Assert
        Assert.Equal(GameStatus.Finished, game.Status);
        Assert.Throws<DomainException>(() => game.MakeMove(_player2Id, 1, 2));
    }

    [Fact]
    public void MakeMove_FinishesGameWithDraw_AfterFullBoard()
    {
        // Arrange
        var game = CreateDefautlGame();

        // Act
        game.MakeMove(_player1Id, 0, 0);
        game.MakeMove(_player2Id, 1, 0);
        game.MakeMove(_player1Id, 0, 1);
        game.MakeMove(_player2Id, 0, 2);
        game.MakeMove(_player1Id, 2, 0);
        game.MakeMove(_player2Id, 1, 1);
        game.MakeMove(_player1Id, 1, 2);
        game.MakeMove(_player2Id, 2, 1);
        game.MakeMove(_player1Id, 2, 2);

        // Assert
        Assert.Equal(GameStatus.Draw, game.Status);
        Assert.Throws<DomainException>(() => game.MakeMove(_player2Id, 0, 0));
    }

    [Fact]
    public void MakeMove_AddsMoveMaidEvent()
    {
        // Arrange
        var game = CreateDefautlGame();

        // Act
        game.MakeMove(_player1Id, 0, 1);

        // Assert
        var @event = game.DomainEvents.Last();
        var moveMaidEvent = Assert.IsType<MoveMaidDomainEvent>(@event);
        Assert.Equal(_player1Id, moveMaidEvent.PlayerId);
        Assert.Equal(0, moveMaidEvent.Row);
        Assert.Equal(1, moveMaidEvent.Column);
        Assert.Equal(Mark.X, moveMaidEvent.Mark);
    }

    [Fact]
    public void MakeMove_AddsMoveMaidEvents()
    {
        // Arrange
        var game = CreateDefautlGame();

        // Act
        game.MakeMove(_player1Id, 0, 0);
        game.MakeMove(_player2Id, 1, 1);

        // Assert
        Assert.Collection(game.DomainEvents, _ => { },
            e => Assert.IsType<MoveMaidDomainEvent>(e),
            e => Assert.IsType<MoveMaidDomainEvent>(e));
    }

    [Fact]
    public void ApplyEvent_GameStartedEvent_InitializesGame()
    {
        // Assert
        var gameStartedEvent = new GameStartedDomainEvent(
            _gameId, _player1Id, _player2Id, BoardSize, WinLength);
        var game = new Game([]);

        // Act
        game.ApplyEvent(gameStartedEvent);

        // Assert
        Assert.Equal(_gameId, game.Id);
        Assert.Equal(_player1Id, game.FirstPlayerId);
        Assert.Equal(_player2Id, game.SecondPlayerId);
        Assert.Equal(_player1Id, game.CurrentPlayer);
        Assert.Equal(1, game.CurrentMove);
        Assert.Equal(WinLength, game.WinLength);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void ApplyEvent_MoveMaidEvent_IncreasesMoveAndChangesCurrentPlayer()
    {
        // Assert
        var moveMaidEvent = new MoveMaidDomainEvent(_player1Id, 0, 0, Mark.X);
        var game = CreateDefautlGame();

        // Act
        game.ApplyEvent(moveMaidEvent);

        // Assert
        Assert.Equal(2, game.CurrentMove);
        Assert.Equal(_player2Id, game.CurrentPlayer);
    }

    [Fact]
    public void ApplyEvent_CheatMoveMaidEvent_IncreasesMoveAndChangesCurrentPlayer()
    {
        // Assert
        var moveMaidEvent = new MoveMaidDomainEvent(_player1Id, 0, 0, Mark.X);
        var game = CreateDefautlGame();

        // Act
        game.ApplyEvent(moveMaidEvent);

        // Assert
        Assert.Equal(2, game.CurrentMove);
        Assert.Equal(_player2Id, game.CurrentPlayer);
    }

    [Fact]
    public void ApplyEvent_GameFinishedEvent_ChangesGameStatusToFinished()
    {
        // Assert   
        var gameFinishedEvent = new GameFinishedDomainEvent(_player1Id, Mark.X);
        var game = CreateDefautlGame();

        // Act
        game.ApplyEvent(gameFinishedEvent);

        // Assert
        Assert.Equal(GameStatus.Finished, game.Status);
    }

    [Fact]
    public void ApplyEvent_GameFinishedWithDrawEvent_ChangesGameStatusToDraw()
    {
        // Assert
        var gameFinishedWithDrawEvent = new GameFinishedWithDrawDomainEvent();
        var game = CreateDefautlGame();

        // Act
        game.ApplyEvent(gameFinishedWithDrawEvent);

        // Assert
        Assert.Equal(GameStatus.Draw, game.Status);
    }
}
