using L.TicTacToe.Domain.Models;
using L.TicTacToe.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace L.TicTacToe.IntegrationTests;

[CollectionDefinition("TicTacToe Test Collection")]
public sealed class TicTacToeCllectionFixture : ICollectionFixture<TicTacToeFixture>;

public sealed class TicTacToeFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private AsyncServiceScope _scope;
    public EventsContext EventsContext { get; private set; } = null!;
    public IEventStore EventStore { get; private set; } = null!;
    public IGameRepository GameRepository { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _scope = Services.CreateAsyncScope();
        EventsContext = _scope.ServiceProvider.GetRequiredService<EventsContext>();
        EventStore = _scope.ServiceProvider.GetRequiredService<IEventStore>();
        GameRepository = _scope.ServiceProvider.GetRequiredService<IGameRepository>();
        await EventsContext.Database.EnsureCreatedAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:Postgres" , "Host=localhost;Port=5432;Database=tic_tac_toe_test;Username=postgres;Password=postgres" },
            });
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await EventsContext.Database.EnsureDeletedAsync();
        await _scope.DisposeAsync();
    }
}
