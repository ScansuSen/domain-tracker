using DomainTracker.DTOs.Domains;
using DomainTracker.DTOs.Favorites;
using DomainTracker.Entities.Models;
using Xunit;

namespace DomainTracker.Tests.Business
{
    public class MappingProfileTests
    {
        [Fact]
        public void Domain_MapsToDomainResponseDto()
        {
            var mapper = TestMapper.Create();
            var domain = new Domain
            {
                Id = 1,
                Name = "example.com",
                IsAvailable = true,
                LastCheckedAt = new DateTime(2026, 1, 1),
                ExpirationDate = null,
            };

            var dto = mapper.Map<DomainResponseDto>(domain);

            Assert.Equal(domain.Id, dto.Id);
            Assert.Equal(domain.Name, dto.Name);
            Assert.Equal(domain.IsAvailable, dto.IsAvailable);
            Assert.Equal(domain.LastCheckedAt, dto.LastCheckedAt);
            Assert.Null(dto.ExpirationDate);
        }

        [Fact]
        public void FavoriteDomain_FlattensRelatedDomainFieldsIntoFavoriteDomainResponseDto()
        {
            var mapper = TestMapper.Create();
            var domain = new Domain
            {
                Id = 1,
                Name = "example.com",
                IsAvailable = false,
                LastCheckedAt = new DateTime(2026, 1, 1),
                ExpirationDate = new DateTime(2030, 1, 1),
            };
            var favorite = new FavoriteDomain
            {
                Id = 9,
                DomainId = 1,
                Domain = domain,
                CreatedAt = new DateTime(2025, 12, 31),
            };

            var dto = mapper.Map<FavoriteDomainResponseDto>(favorite);

            Assert.Equal(favorite.Id, dto.Id);
            Assert.Equal(favorite.CreatedAt, dto.CreatedAt);
            Assert.Equal(domain.Name, dto.DomainName);
            Assert.Equal(domain.IsAvailable, dto.IsAvailable);
            Assert.Equal(domain.LastCheckedAt, dto.LastCheckedAt);
            Assert.Equal(domain.ExpirationDate, dto.ExpirationDate);
        }
    }
}
