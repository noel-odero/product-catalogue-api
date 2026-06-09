namespace ProductCatalogue.DTOs.Dashboard;

public class DashboardSummaryResponse
{
    public int TotalProducts { get; set; }
    public int DraftProducts { get; set; }
    public int ReadyToPublishProducts { get; set; }
    public int PublishedProducts { get; set; }
    public int AssetsPendingReview { get; set; }
    public int RejectedAssets { get; set; }
    public List<RecentAssetResponse> RecentAssets { get; set; } = new();
}

public class RecentAssetResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}