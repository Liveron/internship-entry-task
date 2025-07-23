namespace L.TicTacToe.Infrastructure;

public sealed class EventEntry
{
    public long Id { get; init; }
    public Guid ModelId { get; init; }
    public string Event { get; init; } = null!;
    public string EventType { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public long Version { get; init; }
}
