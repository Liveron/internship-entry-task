using L.TicTacToe.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace L.TicTacToe.IntegrationTests;

[Collection("TicTacToe Test Collection")]
public sealed class ApiTests(TicTacToeFixture fixture) : IDisposable
{
    private const string ETagHeader = "ETag";
    private const string IfMatchHeader = "If-Match";

    private readonly EventsContext _context = fixture.EventsContext;
    private readonly HttpClient _client = fixture.CreateClient();

    private readonly Guid _player1Id = Guid.NewGuid();
    private readonly Guid _player2Id = Guid.NewGuid();

    [Fact]
    public async Task CreateGame_ShouldReturnETag()
    {
        // Arrange
        var content = new { firstPlayerId = _player1Id, secondPlayerId = _player2Id };

        // Act
        var response = await _client.PostAsJsonAsync("tic-tac-toe/api/games", content);
        response.EnsureSuccessStatusCode();
        var eTag = GetETagAsLong(response);

        // Assert
        Assert.Equal(1L, eTag);
    }

    [Fact]
    public async Task GetGame_Forbids_WhenIncorrectPlayer()
    {
        // Arrange & Act
        var (gameId, ETagValues) = await CreateNewGameAsync();

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"tic-tac-toe/api/games/{gameId}?playerId={Guid.NewGuid()}");
        requestMessage.Headers.Add(ETagHeader, ETagValues);

        var response = await _client.SendAsync(requestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MakeMove_ReturnsETag()
    {
        // Arrange & Act
        var (gameId, ETagValues) = await CreateNewGameAsync();

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"tic-tac-toe/api/games/{gameId}/moves")
        {
            Content = JsonContent.Create(new { playerId = _player1Id, row = 0, column = 0 })
        };
        requestMessage.Headers.Add(IfMatchHeader, ETagValues);
        var response = await _client.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();
        
        var eTag = GetETagAsLong(response);

        Assert.Equal(2L, eTag);
    }

    [Fact]
    public async Task MakeMove_ReturnsOkAndSameETagHeader_WhenSameBodySent()
    {
        // Arrange & Act
        var (gameId, ETagValues) = await CreateNewGameAsync();

        var request1Task = MakeMoveAsync(gameId, _player1Id, 0, 0, ETagValues);
        var request2Task = MakeMoveAsync(gameId, _player1Id, 0, 0, ETagValues);

        await Task.WhenAll(request1Task, request2Task);

        var response1 = request1Task.Result;
        var response2 = request2Task.Result;

        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();

        var response1ETag = GetETagAsLong(response1);
        var response2ETag = GetETagAsLong(response2);

        // Assert
        Assert.Equal(response1.StatusCode, response2.StatusCode);
        Assert.Equal(2L, response1ETag);
        Assert.Equal(response1ETag, response2ETag);
    }

    private async Task<(Guid gameId, IEnumerable<string> ETag)> CreateNewGameAsync()
    {
        var content = new { firstPlayerId = _player1Id, secondPlayerId = _player2Id };
        var response = await _client.PostAsJsonAsync("tic-tac-toe/api/games", content);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Guid>(), GetETagAsStringCollection(response));
    }

    private async Task<HttpResponseMessage> MakeMoveAsync(Guid gameId, Guid playerId, int row, int column, IEnumerable<string> ETagValues)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"tic-tac-toe/api/games/{gameId}/moves")
        {
            Content = JsonContent.Create(new { playerId, row, column })
        };
        requestMessage.Headers.Add(IfMatchHeader, ETagValues);
        return await _client.SendAsync(requestMessage);
    }

    private static IEnumerable<string> GetETagAsStringCollection(HttpResponseMessage response)
    {
        return response.Headers.GetValues(ETagHeader);
    }

    private static long GetETagAsLong(HttpResponseMessage response)
    {
        var eTagValue = response.Headers.GetValues(ETagHeader).First().Trim('"');
        return long.Parse(eTagValue);
    }

    public void Dispose()
    {
        _context.EventEntries.ExecuteDelete();
    }
}
