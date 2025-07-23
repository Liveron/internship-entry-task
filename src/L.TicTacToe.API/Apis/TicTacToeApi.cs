using L.TicTacToe.API.Extensions;
using L.TicTacToe.Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace L.TicTacToe.API;

public static class TicTacToeApi
{
    public static IEndpointRouteBuilder MapTicTacToeApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("tic-tac-toe/api")
            .AddExceptionFilter();

        api.MapPost("/games", CreateGameAsync);

        api.MapGet("/games/{gameId:guid}", GetGameAsync);

        api.MapPost("/games/{gameId:guid}/moves", MakeMoveAsync);

        return app;
    }

    public static async Task<Results<Ok<IReadOnlyCollection<IReadOnlyCollection<Mark>>>, BadRequest<string>>> GetGameAsync(
        Guid gameId, [FromQuery] Guid playerId, [AsParameters] ApiServices services)
    {
        var game = await services.GameService.GetGameByIdAsync(playerId, gameId);

        return TypedResults.Ok(game.GetBoard());
    }

    public static async Task<Results<Created<Guid>, BadRequest>> CreateGameAsync(
        CreateGameRequest request, [AsParameters] ApiServices services)
    {
        var gameId = await services.GameService.CreateGameAsync(request.FirstPlayerId, request.SecondPlayerId);

        return TypedResults.Created($"/tic-tac-toe/api/games/{gameId}", gameId);
    }

    public static async Task<Results<Ok, BadRequest<string>>> MakeMoveAsync(
        Guid gameId, MakeMoveRequest request, [AsParameters] ApiServices services)
    {
        await services.GameService.MakeMoveAsync(gameId, request.PlayerId, request.Row, request.Column);

        return TypedResults.Ok();
    }

}

public sealed record CreateGameRequest(Guid FirstPlayerId, Guid SecondPlayerId);
public sealed record MakeMoveRequest(Guid PlayerId, int Row, int Column);


