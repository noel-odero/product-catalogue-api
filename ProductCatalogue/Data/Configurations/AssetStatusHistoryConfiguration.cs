using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalogue.Models;

namespace ProductCatalogue.Data.Configurations;

public class AssetStatusHistoryConfiguration : IEntityTypeConfiguration<AssetStatusHistory>
{
    public void Configure(EntityTypeBuilder<AssetStatusHistory> entity)
    {
        entity.HasKey(h => h.Id);

        entity.Property(h => h.PreviousStatus)
            .HasConversion<string>();

        entity.Property(h => h.NewStatus)
            .HasConversion<string>();

        entity.Property(h => h.Comment)
            .HasMaxLength(500);

        entity.HasOne(h => h.Asset)
            .WithMany(a => a.StatusHistory)
            .HasForeignKey(h => h.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}