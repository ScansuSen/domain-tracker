using DomainTracker.Business.Abstract;
using DomainTracker.Business.Concrete;
using DomainTracker.Core.Results;
using DomainTracker.DataAccess.Abstract;
using DomainTracker.DataAccess.Enums;
using DomainTracker.DataAccess.Models;
using DomainTracker.DTOs.Domains;
using DomainTracker.Entities.Models;
using Moq;
using Xunit;

namespace DomainTracker.Tests.Business
{
    public class FavoriteDomainServiceTests
    {
        private readonly Mock<IFavoriteDomainRepository> _favoriteDomainRepository = new();
        private readonly Mock<IDomainService> _domainService = new();
        private readonly FavoriteDomainService _sut;

        public FavoriteDomainServiceTests()
        {
            _sut = new FavoriteDomainService(_favoriteDomainRepository.Object, _domainService.Object, TestMapper.Create());
        }

        [Fact]
        public async Task AddAsync_WhenDomainCheckFails_PropagatesFailureWithoutPersisting()
        {
            _domainService
                .Setup(s => s.CheckAsync("not_a_valid_domain"))
                .ReturnsAsync(new ErrorDataResult<DomainResponseDto>(400, "Name: 'not_a_valid_domain' is not a valid domain name."));

            var result = await _sut.AddAsync(1, "not_a_valid_domain");

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            _favoriteDomainRepository.Verify(
                r => r.AddFavoriteAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DomainCheckInfo>()),
                Times.Never);
        }

        [Fact]
        public async Task AddAsync_WhenAlreadyFavorite_ReturnsConflictResult()
        {
            var checkedAt = DateTime.UtcNow;
            _domainService
                .Setup(s => s.CheckAsync("example.com"))
                .ReturnsAsync(new SuccessDataResult<DomainResponseDto>(new DomainResponseDto(0, "example.com", true, checkedAt, null)));
            _favoriteDomainRepository
                .Setup(r => r.AddFavoriteAsync(1, "example.com", new DomainCheckInfo(true, checkedAt, null)))
                .ReturnsAsync((AddFavoriteOutcome.AlreadyFavorited, null));

            var result = await _sut.AddAsync(1, "example.com");

            Assert.False(result.Success);
            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task AddAsync_WhenNotYetFavorite_PersistsLinkAndReturnsDto()
        {
            var checkedAt = DateTime.UtcNow;
            _domainService
                .Setup(s => s.CheckAsync("example.com"))
                .ReturnsAsync(new SuccessDataResult<DomainResponseDto>(new DomainResponseDto(0, "example.com", false, checkedAt, new DateTime(2030, 1, 1))));

            var domain = new Domain { Id = 10, Name = "example.com", IsAvailable = false, LastCheckedAt = checkedAt, ExpirationDate = new DateTime(2030, 1, 1) };
            var favorite = new FavoriteDomain { Id = 5, UserId = 1, DomainId = 10, Domain = domain, CreatedAt = DateTime.UtcNow };
            _favoriteDomainRepository
                .Setup(r => r.AddFavoriteAsync(1, "example.com", new DomainCheckInfo(false, checkedAt, new DateTime(2030, 1, 1))))
                .ReturnsAsync((AddFavoriteOutcome.Created, favorite));

            var result = await _sut.AddAsync(1, "example.com");

            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("example.com", result.Data!.DomainName);
            Assert.False(result.Data.IsAvailable);
        }

        [Fact]
        public async Task DeleteAsync_WhenFavoriteBelongsToAnotherUser_ReturnsNotFoundResultAndDoesNotDelete()
        {
            var favorite = new FavoriteDomain { Id = 3, UserId = 2, DomainId = 10 };
            _favoriteDomainRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(favorite);

            var result = await _sut.DeleteAsync(1, 3);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            _favoriteDomainRepository.Verify(r => r.DeleteAsync(It.IsAny<FavoriteDomain>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenFavoriteDoesNotExist_ReturnsNotFoundResult()
        {
            _favoriteDomainRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((FavoriteDomain?)null);

            var result = await _sut.DeleteAsync(1, 99);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task DeleteAsync_WhenOwnedByCaller_DeletesAndReturnsSuccessResult()
        {
            var favorite = new FavoriteDomain { Id = 3, UserId = 1, DomainId = 10 };
            _favoriteDomainRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(favorite);

            var result = await _sut.DeleteAsync(1, 3);

            Assert.True(result.Success);
            _favoriteDomainRepository.Verify(r => r.DeleteAsync(favorite), Times.Once);
        }

        [Fact]
        public async Task RefreshAsync_WhenOwnedByCaller_ReRunsDomainCheckAndReturnsUpdatedInfo()
        {
            var domain = new Domain { Id = 10, Name = "example.com", IsAvailable = true, LastCheckedAt = DateTime.UtcNow.AddDays(-1) };
            var favorite = new FavoriteDomain { Id = 3, UserId = 1, DomainId = 10, Domain = domain, CreatedAt = DateTime.UtcNow.AddDays(-2) };
            _favoriteDomainRepository.Setup(r => r.GetByIdWithDomainAsync(3)).ReturnsAsync(favorite);
            var refreshedAt = DateTime.UtcNow;
            _domainService
                .Setup(s => s.CheckAsync("example.com"))
                .ReturnsAsync(new SuccessDataResult<DomainResponseDto>(new DomainResponseDto(0, "example.com", false, refreshedAt, new DateTime(2031, 1, 1))));
            var expectedCheckInfo = new DomainCheckInfo(false, refreshedAt, new DateTime(2031, 1, 1));
            _favoriteDomainRepository
                .Setup(r => r.RefreshDomainAsync(favorite, expectedCheckInfo))
                .Callback<FavoriteDomain, DomainCheckInfo>((f, checkInfo) =>
                {
                    f.Domain.IsAvailable = checkInfo.IsAvailable;
                    f.Domain.LastCheckedAt = checkInfo.LastCheckedAt;
                    f.Domain.ExpirationDate = checkInfo.ExpirationDate;
                })
                .Returns(Task.CompletedTask);

            var result = await _sut.RefreshAsync(1, 3);

            Assert.True(result.Success);
            Assert.False(result.Data!.IsAvailable);
            Assert.Equal(new DateTime(2031, 1, 1), result.Data.ExpirationDate);
            Assert.Equal(refreshedAt, result.Data.LastCheckedAt);
            _favoriteDomainRepository.Verify(r => r.RefreshDomainAsync(favorite, expectedCheckInfo), Times.Once);
        }

        [Fact]
        public async Task RefreshAsync_WhenFavoriteBelongsToAnotherUser_ReturnsNotFoundResult()
        {
            var domain = new Domain { Id = 10, Name = "example.com" };
            var favorite = new FavoriteDomain { Id = 3, UserId = 2, DomainId = 10, Domain = domain };
            _favoriteDomainRepository.Setup(r => r.GetByIdWithDomainAsync(3)).ReturnsAsync(favorite);

            var result = await _sut.RefreshAsync(1, 3);

            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
            _domainService.Verify(s => s.CheckAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefreshAsync_WhenDomainCheckFails_PropagatesFailureInsteadOfThrowing()
        {
            var domain = new Domain { Id = 10, Name = "example.com", IsAvailable = true, LastCheckedAt = DateTime.UtcNow.AddDays(-1) };
            var favorite = new FavoriteDomain { Id = 3, UserId = 1, DomainId = 10, Domain = domain, CreatedAt = DateTime.UtcNow.AddDays(-2) };
            _favoriteDomainRepository.Setup(r => r.GetByIdWithDomainAsync(3)).ReturnsAsync(favorite);
            _domainService
                .Setup(s => s.CheckAsync("example.com"))
                .ReturnsAsync(new ErrorDataResult<DomainResponseDto>(502, "Unable to reach the domain availability service. Please try again later."));

            var result = await _sut.RefreshAsync(1, 3);

            Assert.False(result.Success);
            Assert.Equal(502, result.StatusCode);
            _favoriteDomainRepository.Verify(
                r => r.RefreshDomainAsync(It.IsAny<FavoriteDomain>(), It.IsAny<DomainCheckInfo>()),
                Times.Never);
        }
    }
}
