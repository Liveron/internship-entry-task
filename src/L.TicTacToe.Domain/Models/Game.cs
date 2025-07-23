using L.TicTacToe.Domain.Events;
using L.TicTacToe.Domain.Exceptions;
using L.TicTacToe.Domain.Setup;

namespace L.TicTacToe.Domain.Models;

#pragma warning disable CS8618
public sealed class Game : Entity<Guid>
{
    private readonly Random _random;
    private Board _board;
    public Guid FirstPlayerId { get; private set; }
    public Guid SecondPlayerId { get; private set; }
    public Guid CurrentPlayer { get; private set; }
    public GameStatus Status { get; private set; }
    public bool IsFinished => Status == GameStatus.Finished;
    public bool IsDraw => Status == GameStatus.Draw;
    public bool IsInProgress => Status == GameStatus.InProgress;
    public int CurrentMove { get; private set; } = 1;
    public int BoardSize => _board.BoardSize;
    public int WinLength { get; private set; }
    public Mark CurrentPlayerMark =>
        CurrentPlayer == FirstPlayerId ? Mark.X : Mark.O;
    public long Version { get; private set; }

    public Game(IEnumerable<IDomainEvent> events)
    {
        _random = new Random();
        // AppendEvents(events);
        ApplyEvents(events);
    }

    public Game(Guid firstPlayerId, Guid secondPlayerId, int boardSize, int winLength = 0, Random? random = null)
    {
        winLength = winLength <= 0 || winLength > boardSize ? boardSize : winLength;

        var gameStartedEvent = new GameStartedDomainEvent(
            Guid.NewGuid(), firstPlayerId, secondPlayerId, boardSize, winLength);

        _random = random ?? new Random();

        AppendEvent(gameStartedEvent);
    }

    public IReadOnlyCollection<IReadOnlyCollection<Mark>> GetBoard()
    {
        return _board.Cells;
    }

    public void MakeMove(Guid playerId, int row, int column)
    {
        if (!IsInProgress)
            throw new DomainException($"Игра уже закончена.");

        if (playerId != CurrentPlayer)
            throw new DomainException($"Сейчас ход другого игрока.");

        IDomainEvent? moveMaidEvent = null;

        if (CurrentMove % 3 == 0)
        {
            moveMaidEvent = TryToMakeCheatMove(playerId, row, column);
        }

        moveMaidEvent ??= new MoveMaidDomainEvent(playerId, row, column, CurrentPlayerMark);
        AppendEvent(moveMaidEvent);

        var winMark = CheckWinFromCell(row, column);

        if (winMark != Mark.Empty)
        {
            var winnerId = winMark == Mark.X ? FirstPlayerId : SecondPlayerId;
            var gameFinishedEvent = new GameFinishedDomainEvent(winnerId, winMark);
            AppendEvent(gameFinishedEvent);
        }
        else if (_board.IsFull)
        {
            var gameFinishedWithDrawEvent = new GameFinishedWithDrawDomainEvent();
            AppendEvent(gameFinishedWithDrawEvent);
        }
    }

    private CheatMoveMaidDomainEvent? TryToMakeCheatMove(Guid playerId, int row, int column)
    {
        if (_random.NextDouble() < 0.1)
        {
            var cheatMark = CurrentPlayerMark == Mark.X ? Mark.O : Mark.X;
            return new CheatMoveMaidDomainEvent(playerId, row, column, CurrentPlayerMark, cheatMark);
        }
        return null;
    }

    private Mark CheckWinFromCell(int row, int column)
    {
        var mark = _board.GetCell(row, column);

        if (mark == Mark.Empty)
            return Mark.Empty;

        if (CheckWinFromCellForAllDirections(mark, row, column))
            return mark;

        return Mark.Empty;
    }

    private bool CheckWinFromCellForAllDirections(Mark mark, int row, int column)
    {
        return CheckWinFromCellForDirection(mark, row, column, Direction.Horizontal)
            || CheckWinFromCellForDirection(mark, row, column, Direction.Vertical)
            || CheckWinFromCellForDirection(mark, row, column, Direction.MainDiagonal)
            || CheckWinFromCellForDirection(mark, row, column, Direction.SecondaryDiagonal);
    }

    private bool CheckWinFromCellForDirection(Mark mark, int row, int column, Direction direction)
    {
        var (deltaRow, deltaColumn) = GetDeltasForDirection(direction);

        var counterForWin = 1;
        counterForWin += CountDirectionFromCell(mark, row, column, deltaRow, deltaColumn);

        if (counterForWin == WinLength)
            return true;

        counterForWin += CountDirectionFromCell(mark, row, column, -deltaRow, -deltaColumn);

        if (counterForWin >= WinLength)
            return true;

        return false;
    }

    private static (int deltaRow, int deltaColumn) GetDeltasForDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Horizontal => (0, 1),
            Direction.Vertical => (1, 0),
            Direction.MainDiagonal => (1, 1),
            Direction.SecondaryDiagonal => (-1, 1),
            _ => throw new InvalidOperationException("Неизвестное направление.")
        };
    }

    private int CountDirectionFromCell(Mark mark, int row, int column, int deltaRow, int deltaColumn)
    {
        var counter = 0;
        row += deltaRow;
        column += deltaColumn;

        while(counter < WinLength && IsValidPosition(row, column) && _board.GetCell(row, column) == mark)
        {
            counter++;
            row += deltaRow;
            column += deltaColumn;
        }
        return counter;
    }

    private bool IsValidPosition(int row, int column)
    {
        return row >= 0 && row < BoardSize && column >= 0 && column < BoardSize;
    }

    private void AppendEvent(IDomainEvent @event)
    {
        ApplyEvent(@event);
        AddDomainEvent(@event);
    }

    private void ApplyEvents(IEnumerable<IDomainEvent> events)
    {
        foreach (var @event in events)
        {
            ApplyEvent(@event);
        }
    }

    public override void ApplyEvent(IDomainEvent @event)
    {
        Apply((dynamic)@event);
        Version++;
    }

    private void Apply(GameStartedDomainEvent @event)
    {
        Id = @event.GameId;
        _board = new Board(@event.BoardSize);
        WinLength = @event.WinLength;
        FirstPlayerId = @event.FirstPlayerId;
        SecondPlayerId = @event.SecondPlayerId;
        CurrentPlayer = @event.FirstPlayerId; // Первый ход делает первый игрок
        Status = GameStatus.InProgress;
    }

    private void Apply(MoveMaidDomainEvent @event)
    {
        if (@event.Mark == Mark.X)
        {
            _board.SetCellX(@event.Row, @event.Column);
        }
        else 
        {
            _board.SetCellO(@event.Row, @event.Column);
        }

        CurrentMove++;
        CurrentPlayer = @event.PlayerId == FirstPlayerId ? SecondPlayerId : FirstPlayerId;     
    }

    private void Apply(CheatMoveMaidDomainEvent @event)
    {
        if(@event.CheatMark == Mark.X)
        {
            _board.SetCellX(@event.Row, @event.Column);         
        }
        else
        {
            _board.SetCellO(@event.Row, @event.Column);
        }

        CurrentMove++;
        CurrentPlayer = @event.PlayerId == FirstPlayerId ? SecondPlayerId : FirstPlayerId;
    }

    private void Apply(GameFinishedDomainEvent _)
    {
        Status = GameStatus.Finished;
    }

    private void Apply(GameFinishedWithDrawDomainEvent _)
    {
        Status = GameStatus.Draw;
    }

    enum Direction
    {
        Horizontal,
        Vertical,
        MainDiagonal,
        SecondaryDiagonal
    }
}
