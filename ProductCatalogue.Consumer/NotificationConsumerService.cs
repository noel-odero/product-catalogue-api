using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductCatalogue.Contracts;

namespace ProductCatalogue.Consumer;

public class NotificationConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaConsumerSettings _settings;
    private readonly ILogger<NotificationConsumerService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public NotificationConsumerService(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaConsumerSettings> settings,
        ILogger<NotificationConsumerService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_settings.AssetEventsTopic);

        _logger.LogInformation(
            "Consumer subscribed to {Topic} as group {Group}",
            _settings.AssetEventsTopic, _settings.GroupId);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? result;
                try
                {
                    result = consumer.Consume(stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogWarning(ex, "Consume error, continuing");
                    continue;
                }

                if (result?.Message is null)
                    continue;

                try
                {
                    await HandleMessage(result.Message.Value, stoppingToken);

                    consumer.Commit(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to process message at offset {Offset}, will retry",
                        result.Offset);
                }
            }
        }
        catch (OperationCanceledException){}
        finally
        {
            consumer.Close();
            _logger.LogInformation("Consumer closed");
        }
    }

    private async Task HandleMessage(string rawMessage, CancellationToken ct)
    {
        EventEnvelope<JsonElement>? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<EventEnvelope<JsonElement>>(rawMessage, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Skipping malformed message: {Raw}", rawMessage);
            return;
        }

        if (envelope is null)
            return;

        if (envelope.EventType is not ("AssetApproved" or "AssetRejected"))
            return;
        AssetEventFields? fields;
        try
        {
            fields = JsonSerializer.Deserialize<AssetEventFields>(
                envelope.Payload.GetRawText(), JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Skipping message with unreadable payload");
            return;
        }

        if (fields is null || fields.AssetId == Guid.Empty || fields.ProductId == Guid.Empty)
        {
            _logger.LogWarning("Message payload missing required fields, skipping");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var alreadyLogged = await db.NotificationLogs
            .AnyAsync(n => n.EventId == envelope.EventId, ct);

        if (alreadyLogged)
        {
            _logger.LogInformation("Duplicate event {EventId} ignored", envelope.EventId);
            return;
        }

        db.NotificationLogs.Add(new NotificationLog
        {
            EventId = envelope.EventId,
            EventType = envelope.EventType,
            AssetId = fields.AssetId,
            ProductId = fields.ProductId,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        try
        {
            await db.SaveChangesAsync(ct);
            _logger.LogInformation(
                "Logged {EventType} for asset {AssetId}",
                envelope.EventType, fields.AssetId);
        }
        catch (DbUpdateException)
        {
            _logger.LogInformation(
                "Duplicate event {EventId} ignored (unique constraint)", envelope.EventId);
        }
    }
    private sealed record AssetEventFields(Guid AssetId, Guid ProductId);
}