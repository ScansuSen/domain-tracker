using DomainTracker.Core.Results;
using DomainTracker.DTOs.Auth;

namespace DomainTracker.Business.Abstract
{
    public interface IAuthService
    {
        Task<IDataResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);

        Task<IDataResult<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    }
}
