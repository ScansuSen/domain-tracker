using AutoMapper;
using DomainTracker.DTOs.Favorites;
using DomainTracker.Entities.Models;

namespace DomainTracker.Business.Mapping
{
    public class FavoriteDomainProfile : Profile
    {
        public FavoriteDomainProfile()
        {
            CreateMap<FavoriteDomain, FavoriteDomainResponseDto>()
                .ForCtorParam("domainName", opt => opt.MapFrom(src => src.Domain.Name))
                .ForCtorParam("isAvailable", opt => opt.MapFrom(src => src.Domain.IsAvailable))
                .ForCtorParam("lastCheckedAt", opt => opt.MapFrom(src => src.Domain.LastCheckedAt))
                .ForCtorParam("expirationDate", opt => opt.MapFrom(src => src.Domain.ExpirationDate));
        }
    }
}
