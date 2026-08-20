namespace DomainTracker.Business.Models
{
    public record RdapLookupResult(bool IsAvailable, DateTime? ExpirationDate);
}
