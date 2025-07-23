using L.TicTacToe.API;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.MapApplicationServicesEndpoints();

app.MapTicTacToeApi();

app.Run();