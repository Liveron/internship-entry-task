using L.TicTacToe.Domain.Setup;
using L.TicTacToe.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;

namespace L.TicTacToe.Infrastructure;

public sealed class EventStore(EventsContext context) : IEventStore
{
    private readonly EventsContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<List<IDomainEvent>> GetEventsAsync(Guid modelId)
    {
        var eventEntries = await _context.EventEntries.AsNoTracking()
            .Where(e => e.ModelId == modelId)
            .OrderBy(e => e.Version)
            .ToListAsync();

        return eventEntries.Count == 0 ? [] : DeserializeEventEntries(eventEntries);
    }

    private static List<IDomainEvent> DeserializeEventEntries(IEnumerable<EventEntry> eventEntries)
    {
        return [.. eventEntries.Select(DeserializeEvent)];
    }

    private static IDomainEvent DeserializeEvent(EventEntry eventEntry)
    {
        return (IDomainEvent)JsonSerializer.Deserialize(eventEntry.Event, Type.GetType(eventEntry.EventType)!)!; // Не очень
    }

    public async Task<long> SaveEventsAsync(IEnumerable<IDomainEvent> events, Guid modelId, long expectedVersion)
    {
        if (expectedVersion < 0)
            throw new InvalidOperationException("Expected version couldn't be negative.");

        if (!events.Any())
            return expectedVersion;

        var (eventEntries, version) = CreateEventEntries(events, modelId, expectedVersion);

        await _context.EventEntries.AddRangeAsync(eventEntries);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgex && pgex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                var actualVersion = await GetEventStreamVersionAsync(modelId);
                throw new ConcurrencyException(expectedVersion, actualVersion);
            }
        }

        return version;
    }

    public async Task<long> GetEventStreamVersionAsync(Guid modelId)
    {
        return await _context.EventEntries.AsNoTracking()
            .Where(e => e.ModelId.Equals(modelId))
            .MaxAsync(e => (long?)e.Version) ?? 0;
    }

    private static (List<EventEntry> eventEntries, long version) CreateEventEntries(
        IEnumerable<IDomainEvent> events, Guid modelId, long startVersion)
    {
        var eventEntries = new List<EventEntry>();

        var version = startVersion;
        foreach (var @event in events)
        {
            version++;
            eventEntries.Add(CreateEventEntry(@event, modelId, version));
        }

        return (eventEntries, version);
    }

    private static EventEntry CreateEventEntry(IDomainEvent @event, Guid modelId, long version)
    {
        return new EventEntry
        {
            ModelId = modelId,
            EventType = @event.GetType().AssemblyQualifiedName!, // Типы используются в одном процессе, поэтому пойдет
            Event = JsonSerializer.Serialize(@event, @event.GetType()),
            CreatedAt = DateTime.UtcNow,
            Version = version,
        };
    }
}
