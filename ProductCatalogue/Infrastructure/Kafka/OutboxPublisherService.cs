using Microsoft.EntityFrameworkCore;
using ProductCatalogue.Data;
using ProductCatalogue.Models;

namespace ProductCatalogue.Infrastructure.Kafka;

public class OutboxPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IKafkaProducer _producer;
    private readonly ILogger<OutboxPublisherService> _logger;

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;
    private const int MaxAttempts = 5;

    public OutboxPublisherService(
        IServiceScopeFactory scopeFactory,
        IKafkaProducer producer,
        ILogger<OutboxPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _producer = producer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DrainOnce(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox drain cycle failed");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task DrainOnce(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pending = await context.OutboxMessages
            .Where(m => m.Status == OutboxStatus.Pending)
            .OrderBy(m => m.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (pending.Count == 0)
            return;

        foreach (var message in pending)
        {
            try
            {
                await _producer.ProduceAsync(message.Topic, message.Key, message.Payload, ct);

                message.Status = OutboxStatus.Published;
                message.PublishedAt = DateTimeOffset.UtcNow;
                message.Attempts++;
            }
            catch (Exception ex)
            {
                message.Attempts++;

                if (message.Attempts >= MaxAttempts)
                {
                    message.Status = OutboxStatus.Failed;
                    _logger.LogError(ex,
                        "Outbox message {Id} PARKED after {Attempts} failed attempts. " +
                        "Event {EventType} will not be retried and needs manual attention.",
                        message.Id, message.Attempts, message.EventType);
                }
                else
                {
                    _logger.LogWarning(ex,
                        "Failed to publish outbox message {Id} (attempt {Attempts}/{Max})",
                        message.Id, message.Attempts, MaxAttempts);
                }
            }
        }

        await context.SaveChangesAsync(ct);
    }
}