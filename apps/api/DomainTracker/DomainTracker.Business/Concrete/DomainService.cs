using AutoMapper;
using DomainTracker.Business.Abstract;
using DomainTracker.Business.Validation;
using DomainTracker.Core.Constants;
using DomainTracker.Core.Results;
using DomainTracker.Core.Utilities;
using DomainTracker.DataAccess.Abstract;
using DomainTracker.DTOs.Domains;
using DomainTracker.Entities.Models;

namespace DomainTracker.Business.Concrete
{
    public class DomainService : IDomainService
    {
        private static readonly DomainNameValidator DomainNameValidator = new();

        private readonly IDomainRepository _domainRepository;
        private readonly IRdapClient _rdapClient;
        private readonly IMapper _mapper;

        public DomainService(IDomainRepository domainRepository, IRdapClient rdapClient, IMapper mapper)
        {
            _domainRepository = domainRepository;
            _rdapClient = rdapClient;
            _mapper = mapper;
        }

        public async Task<IDataResult<DomainResponseDto>> CheckAsync(string domainName)
        {
            var normalizedName = DomainNameNormalizer.Normalize(domainName);
            var validationResult = DomainNameValidator.Validate(normalizedName);
            if (!validationResult.IsValid)
                return new ErrorDataResult<DomainResponseDto>(HttpStatusCodes.BadRequest, validationResult.ToMessages());

            var rdapResult = await _rdapClient.LookupAsync(normalizedName);
            var now = DateTime.UtcNow;

            var domain = await _domainRepository.GetByNameAsync(normalizedName);
            if (domain is null)
            {
                domain = new Domain
                {
                    Name = normalizedName,
                    IsAvailable = rdapResult.IsAvailable,
                    LastCheckedAt = now,
                    ExpirationDate = rdapResult.ExpirationDate,
                };
                await _domainRepository.AddAsync(domain);
            }
            else
            {
                domain.IsAvailable = rdapResult.IsAvailable;
                domain.LastCheckedAt = now;
                domain.ExpirationDate = rdapResult.ExpirationDate;
                domain.UpdatedAt = now;
                await _domainRepository.UpdateAsync(domain);
            }

            return new SuccessDataResult<DomainResponseDto>(_mapper.Map<DomainResponseDto>(domain));
        }
    }
}
