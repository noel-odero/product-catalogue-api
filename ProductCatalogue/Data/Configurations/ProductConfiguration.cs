using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalogue.Models;

namespace ProductCatalogue.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> entity)
    {
        entity.HasKey(p=> p.Id); // primary key
        entity.HasIndex(p => p.ProductCode).IsUnique();
        entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
        entity.Property(p => p.ProductCode).IsRequired().HasMaxLength(50);
        entity.Property(p => p.Description).HasMaxLength(2000);
        entity.Property(p=>p.Brand).IsRequired().HasMaxLength(100);
        entity.Property(p => p.Category).IsRequired().HasMaxLength(100);
        entity.Property(p=> p.TargetMarket).IsRequired().HasMaxLength(100);
        entity.Property(p => p.Season).IsRequired().HasMaxLength(50);
        entity.Property(p => p.Status).HasConversion<string>();
    }
}