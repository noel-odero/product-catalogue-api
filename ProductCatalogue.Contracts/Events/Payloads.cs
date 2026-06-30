namespace ProductCatalogue.Contracts;
public record AssetUploadedPayload(
    Guid AssetId,
    Guid ProductId,
    Guid? VariantId,
    string AssetType,
    string Title,
    Guid UploadedBy,
    DateTimeOffset UploadedAt);

public record AssetApprovedPayload(
    Guid AssetId,
    Guid ProductId,
    string AssetType,
    Guid ApprovedBy,
    DateTimeOffset ApprovedAt);

public record AssetRejectedPayload(
    Guid AssetId,
    Guid ProductId,
    string AssetType,
    string Reason,
    Guid RejectedBy,
    DateTimeOffset RejectedAt);

public record ProductSubmittedForReviewPayload(
    Guid ProductId,
    string ProductCode,
    string Name,
    int AssetCount,
    DateTimeOffset SubmittedAt);

public record ProductPublishedPayload(
    Guid ProductId,
    string ProductCode,
    string Name,
    DateTimeOffset PublishedAt);