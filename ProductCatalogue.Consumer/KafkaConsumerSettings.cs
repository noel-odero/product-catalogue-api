namespace ProductCatalogue.Consumer;

public class KafkaConsumerSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string AssetEventsTopic { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
}