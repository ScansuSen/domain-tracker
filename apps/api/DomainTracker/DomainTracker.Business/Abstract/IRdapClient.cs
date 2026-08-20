using DomainTracker.Business.Models;

namespace DomainTracker.Business.Abstract
{
    public interface IRdapClient
    {
        Task<RdapLookupResult> LookupAsync(string domainName, CancellationToken cancellationToken = default);
    }
}
