namespace ProductCatalogue.Services;

public interface IEventPublisher
{
    // writes an event to the outbox in the current transaction.
    void Enqueue<TPayload>(
        string topic,
        string key,
        string eventType,
        TPayload payload);
}