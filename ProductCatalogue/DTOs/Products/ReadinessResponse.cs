namespace ProductCatalogue.DTOs.Products;

public class ReadinessResponse
{
    public Guid ProductId { get; init; }
    public List<ReadinessCheckResponse> Checks { get; init; } = new();
    public int PassedCount { get; init; }
    public int TotalCount { get; init; }
    public bool CanPublish { get; init; }
}

public class ReadinessCheckResponse
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public bool Passed { get; init; }
}