using L.TicTacToe.Domain.Models;
using L.TicTacToe.Domain.Setup;

namespace L.TicTacToe.Domain.Events;

public sealed record MoveMaidDomainEvent(
    Guid PlayerId, int Row, int Column, Mark Mark) : IDomainEvent;
