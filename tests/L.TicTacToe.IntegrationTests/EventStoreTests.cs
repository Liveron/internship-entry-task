using L.TicTacToe.Domain.Setup;
using L.TicTacToe.Infrastructure;
using L.TicTacToe.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace L.TicTacToe.IntegrationTests;

[Collection("TicTacToe Test Collection")]
public sealed class EventStoreTests(TicTacToeFixture fixture) : IDisposable
{
    private readonly Guid _modelId = Guid.NewGuid();

    private readonly IEventStore _eventStore = fixture.EventStore;
    private readonly EventsContext _context = fixture.EventsContext;

    [Fact]
    public async Task SaveEventsAsync_ShouldSaveEventsToDatabase()
    {
        // Arrange
        var events = new List<IDomainEvent> 
        {
            new TestEvent("Event 1"),
        };

        // Act
        await _eventStore.SaveEventsAsync(events, _modelId);

        // Assert
        var savedEvents = await _eventStore.GetEventsAsync(_modelId);
        Assert.Equal(events.Count, savedEvents.Count);
        Assert.Equal(events[0].GetType(), savedEvents[0].GetType());
        Assert.Equal(((TestEvent)events[0]).Data, ((TestEvent)savedEvents[0]).Data);
    }

    [Fact]
    public async Task SaveEventsAsync_ShouldThrowConcurrencyException_WhenSuchVersionAlreadyExists()
    {
        // Arrange
        var events = new List<IDomainEvent>
        {
            new TestEvent("Event 1"),
        };
        await _eventStore.SaveEventsAsync(events, _modelId);

        // Act & Assert
        await Assert.ThrowsAsync<ConcurrencyException>(async () =>
        {
            await _eventStore.SaveEventsAsync(events, _modelId);
        });
    }

    [Fact]
    public async Task GetEventsAsync_ShouldReturnEventsWithVersionOrder()
    {
        // Arrange
        var events = new List<TestEvent>
        {
            new("Event 1"),
            new("Event 2"),
            new("Event 3")
        };
        await _eventStore.SaveEventsAsync(events, _modelId);

        // Act
        var result = await _eventStore.GetEventsAsync(_modelId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(events[0].Data, ((TestEvent)result[0]).Data);
        Assert.Equal(events[1].Data, ((TestEvent)result[1]).Data);
        Assert.Equal(events[2].Data, ((TestEvent)result[2]).Data);
    }

    [Fact]
    public async Task GetEventStreamVersionAsync_ShouldReturnCorrectVersion()
    {
        // Arrange
        var events = new List<TestEvent>()
        {
            new("Event 1"),
            new("Event 2"),
            new("Event 3")
        };
        await _eventStore.SaveEventsAsync(events, _modelId);

        // Act
        var version = await _eventStore.GetEventStreamVersionAsync(_modelId);

        Assert.Equal(3, version);
    }

    [Fact]
    public async Task GetEventStreamVersionAsync_ShouldReturnZero_WhenNoEventsExist()
    {
        // Arrange
        var nonExistentId = _modelId;

        // Act
        var version = await _eventStore.GetEventStreamVersionAsync(nonExistentId);

        // Assert
        Assert.Equal(0, version);
    }

    public void Dispose()
    {
        _context.EventEntries.ExecuteDelete();
    }

    record TestEvent(string Data) : IDomainEvent;
}
