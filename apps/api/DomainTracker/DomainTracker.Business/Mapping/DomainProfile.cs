using AutoMapper;
using DomainTracker.DTOs.Domains;
using DomainTracker.Entities.Models;

namespace DomainTracker.Business.Mapping
{
    public class DomainProfile : Profile
    {
        public DomainProfile()
        {
            CreateMap<Domain, DomainResponseDto>();
        }
    }
}
