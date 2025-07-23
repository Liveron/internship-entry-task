using L.TicTacToe.Domain.Models;
using L.TicTacToe.Domain.Setup;

namespace L.TicTacToe.Domain.Events;

public sealed record GameFinishedDomainEvent(Guid Winner, Mark WinnerMark) : IDomainEvent;
