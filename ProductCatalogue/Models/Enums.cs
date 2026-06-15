namespace ProductCatalogue.Models;

public enum ProductStatus
{
    Draft,
    InReview,
    ReadyToPublish,
    Published,
    Archived
}

public enum AssetType
{
    MainImage,
    VariantImage,
    LifestyleImage,
    MarketingBanner,
    SizeGuide,
    TechnicalDocument
}

public enum AssetStatus
{
    Uploaded,
    PendingReview,
    Approved,
    Rejected,
    Archived
    
}

public enum VariantStatus
{
    Active,
    Archived
}