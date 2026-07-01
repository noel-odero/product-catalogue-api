using Microsoft.EntityFrameworkCore;

namespace ProductCatalogue.Consumer;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options) { }

    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationLog>(b =>
        {
            b.ToTable("NotificationLogs");   // match the API's table name exactly
            b.HasKey(n => n.Id);
            b.HasIndex(n => n.EventId).IsUnique();
        });
    }
}