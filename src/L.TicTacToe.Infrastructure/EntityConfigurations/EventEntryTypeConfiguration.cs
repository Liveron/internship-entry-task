using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L.TicTacToe.Infrastructure.EntityConfigurations;

class EventEntryTypeConfiguration : IEntityTypeConfiguration<EventEntry>
{
    public void Configure(EntityTypeBuilder<EventEntry> eventEntryConfiguration)
    {
        eventEntryConfiguration.ToTable("events");

        eventEntryConfiguration.HasKey(e => e.Id);

        eventEntryConfiguration.HasIndex(e => new { e.ModelId, e.Version })
            .IsUnique();

        eventEntryConfiguration.Property(e => e.Event)
            .HasColumnType("jsonb"); 
    }
}
