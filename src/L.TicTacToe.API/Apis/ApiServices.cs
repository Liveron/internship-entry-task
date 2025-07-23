using L.TicTacToe.API.Application.Services;

namespace L.TicTacToe.API;

public sealed class ApiServices(IGamesService gameService)
{
    public IGamesService GameService => gameService;
}
