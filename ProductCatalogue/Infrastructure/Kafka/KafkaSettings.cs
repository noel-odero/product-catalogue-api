namespace ProductCatalogue.Infrastructure.Kafka;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string AssetEventsTopic { get; set; } = string.Empty;
    public string ProductEventsTopic { get; set; } = string.Empty;
}