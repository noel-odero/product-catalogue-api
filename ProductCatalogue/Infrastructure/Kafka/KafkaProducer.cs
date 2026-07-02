using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace ProductCatalogue.Infrastructure.Kafka;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(IOptions<KafkaSettings> settings)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = settings.Value.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 200,
            MessageTimeoutMs = 5000,
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync(string topic, string key, string value, CancellationToken ct)
    {
        await _producer.ProduceAsync(
            topic,
            new Message<string, string> { Key = key, Value = value },
            ct);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}