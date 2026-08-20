namespace DomainTracker.Core.Utilities;

public static class DomainNameNormalizer
{
    public static string Normalize(string domainName) => domainName.Trim().ToLowerInvariant();
}
