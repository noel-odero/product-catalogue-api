using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalogue.Models;

namespace ProductCatalogue.Data.Configurations;
public class VariantConfigurations: IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> entity)
    {
        entity.HasKey(v => v.Id);
        entity.HasIndex(v => new {v.ProductId, v.VariantCode}).IsUnique();
        entity.Property(v => v.Name ).IsRequired().HasMaxLength(200);
        entity.Property(v => v.VariantCode).IsRequired().HasMaxLength(50);
        entity.Property(v => v.Colour).IsRequired().HasMaxLength(50);
        entity.Property(v => v.Size ).IsRequired().HasMaxLength(50);
        entity.Property(v => v.Material ).IsRequired().HasMaxLength(50);
        entity.Property(v => v.Barcode ).IsRequired().HasMaxLength(100);
        entity.Property(v => v.Status ).HasConversion<string>();
        entity.HasOne(v => v.Product).WithMany(p => p.Variants).HasForeignKey(v => v.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}