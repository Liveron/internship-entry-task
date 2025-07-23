using L.TicTacToe.Domain.Setup;

namespace L.TicTacToe.Domain.Events;

public sealed record GameStartedDomainEvent(
    Guid GameId, Guid FirstPlayerId, Guid SecondPlayerId, int BoardSize, int WinLength)
    : IDomainEvent;