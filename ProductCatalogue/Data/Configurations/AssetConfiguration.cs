using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalogue.Models;

namespace ProductCatalogue.Data.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> entity)
    {
        entity.HasKey(a => a.Id);

        entity.Property(a => a.OriginalFileName)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(a => a.StoragePath)
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(a => a.Description)
            .HasMaxLength(1000);

        entity.Property(a => a.AssetType)
            .HasConversion<string>();

        entity.Property(a => a.Status)
            .HasConversion<string>();

        entity.HasOne(a => a.Product)
            .WithMany(p => p.Assets)
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(a => a.Variant)
            .WithMany(v => v.Assets)
            .HasForeignKey(a => a.VariantId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}