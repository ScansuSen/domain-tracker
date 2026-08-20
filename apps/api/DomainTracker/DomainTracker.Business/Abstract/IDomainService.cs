using DomainTracker.Core.Results;
using DomainTracker.DTOs.Domains;

namespace DomainTracker.Business.Abstract
{
    public interface IDomainService
    {
        Task<IDataResult<DomainResponseDto>> CheckAsync(string domainName);
    }
}
