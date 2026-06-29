namespace ProductCatalogue.Services;

public interface IEventPublisher
{
    void Enqueue<TPayload>(
        string topic,
        string key,
        string eventType,
        TPayload payload);
}