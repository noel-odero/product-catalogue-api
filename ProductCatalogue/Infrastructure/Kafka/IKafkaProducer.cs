namespace ProductCatalogue.Infrastructure.Kafka;

public interface IKafkaProducer
{
    Task ProduceAsync(string topic, string key, string value, CancellationToken ct);
}