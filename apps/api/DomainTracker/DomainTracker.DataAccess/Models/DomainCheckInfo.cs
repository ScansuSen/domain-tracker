namespace DomainTracker.DataAccess.Models
{
    /// <summary>
    /// The fields a fresh RDAP check produces, needed to create/update a Domain row. Groups them
    /// so AddFavoriteAsync/RefreshDomainAsync don't take three separate primitive parameters.
    /// </summary>
    public record DomainCheckInfo(bool IsAvailable, DateTime LastCheckedAt, DateTime? ExpirationDate);
}
