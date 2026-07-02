namespace ProductCatalogue.Contracts;
public class EventEnvelope<TPayload>
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public int Version { get; init; } = 1;
    public TPayload Payload { get; init; } = default!;
}