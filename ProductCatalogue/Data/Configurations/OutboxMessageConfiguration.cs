using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalogue.Models;

namespace ProductCatalogue.Data.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Topic).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Key).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Payload).IsRequired();
        builder.Property(o => o.EventType).IsRequired().HasMaxLength(100);
        builder.HasIndex(o => new { o.Published, o.OccurredAt });
    }
}