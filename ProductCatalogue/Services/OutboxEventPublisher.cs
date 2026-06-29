using System.Text.Json;
using ProductCatalogue.Data;
using ProductCatalogue.Events;
using ProductCatalogue.Models;

namespace ProductCatalogue.Services;

public class OutboxEventPublisher : IEventPublisher
{
    private readonly AppDbContext _context;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public OutboxEventPublisher(AppDbContext context)
    {
        _context = context;
    }

    public void Enqueue<TPayload>(
        string topic,
        string key,
        string eventType,
        TPayload payload)
    {
        var envelope = new EventEnvelope<TPayload>
        {
            EventType = eventType,
            Payload = payload,
        };

        var json = JsonSerializer.Serialize(envelope, JsonOptions);

        _context.OutboxMessages.Add(new OutboxMessage
        {
            Topic = topic,
            Key = key,
            EventType = eventType,
            Payload = json,
            OccurredAt = envelope.OccurredAt,
        });
    }
}