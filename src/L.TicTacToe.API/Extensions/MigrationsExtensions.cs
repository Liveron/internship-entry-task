using Microsoft.EntityFrameworkCore;

namespace L.TicTacToe.API.Extensions;

internal static class MigrationsExtensions
{
    public static void AddMigrations<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddHostedService<MigrationsBackgroundService<TContext>>();
    }

    private class MigrationsBackgroundService<TContext>(IServiceProvider serviceProvider) 
        : BackgroundService where TContext : DbContext
    {
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            try
            {
                await context.Database.MigrateAsync(cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken _)
        {
            return Task.CompletedTask;
        }
    }
}
