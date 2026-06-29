using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalogue.Models;

namespace ProductCatalogue.Data.Configurations;

public class AssetTagConfiguration : IEntityTypeConfiguration<AssetTag>
{
    public void Configure(EntityTypeBuilder<AssetTag> entity)
    {
        entity.HasKey(t => t.Id);

        entity.Property(t => t.Tag)
            .IsRequired()
            .HasMaxLength(50);

        entity.HasOne(t => t.Asset)
            .WithMany(a => a.Tags)
            .HasForeignKey(t => t.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        // a tag can't appear twice on the same asset
        entity.HasIndex(t => new { t.AssetId, t.Tag }).IsUnique();
    }
}