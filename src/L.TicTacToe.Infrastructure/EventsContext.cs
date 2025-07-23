using L.TicTacToe.Infrastructure.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace L.TicTacToe.Infrastructure;

public sealed class EventsContext(DbContextOptions<EventsContext> options) : DbContext(options)
{
    public DbSet<EventEntry> EventEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EventEntryTypeConfiguration());
    }
}
