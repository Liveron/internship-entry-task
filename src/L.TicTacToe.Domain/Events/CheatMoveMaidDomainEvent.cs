using L.TicTacToe.Domain.Models;
using L.TicTacToe.Domain.Setup;

namespace L.TicTacToe.Domain.Events;

public sealed record CheatMoveMaidDomainEvent(
    Guid PlayerId, int Row, int Column, Mark CorrectMark, Mark CheatMark) : IDomainEvent;
