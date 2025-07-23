using L.TicTacToe.API;
using L.TicTacToe.API.Application.Services;
using L.TicTacToe.API.Extensions;
using L.TicTacToe.API.Options;
using L.TicTacToe.Domain.Models;
using L.TicTacToe.Infrastructure;
using L.TicTacToe.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

internal static class DependencyInjectionExtensions
{
    private const string ConnectionStringKey = "Postgres";

    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHttpContextAccessor();

        services.AddDbContext<EventsContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString(ConnectionStringKey);
            options.UseNpgsql(connectionString);
        });

        services.AddMigrations<EventsContext>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c => c.OperationFilter<IfMatchVersionOperationFilter>());

        services.AddHealthChecks();

        var section = builder.Configuration.GetSection(GameOptions.SectionKey);
        services.Configure<GameOptions>(section);

        services.AddScoped<IEventStore, EventStore>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGamesService, GamesService>();
    }
}

internal sealed class IfMatchVersionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.Name == nameof(TicTacToeApi.MakeMoveAsync))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "If-Match",
                In = ParameterLocation.Header,
                Description = "ETag версия игры для обработок ошибок идемпотентности и конкурентности. " +
                "Значение должно быть включено в двойные кавычки, например \"1\"",
                Required = true,
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
    }
}
