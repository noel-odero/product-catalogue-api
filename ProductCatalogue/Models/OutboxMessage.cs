namespace ProductCatalogue.Models;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Topic { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;
    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PublishedAt { get; set; }
    public int Attempts { get; set; } = 0;
}