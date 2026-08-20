using DomainTracker.Business.Abstract;
using DomainTracker.Business.Concrete;
using DomainTracker.Business.Models;
using DomainTracker.DataAccess.Abstract;
using DomainTracker.Entities.Models;
using Moq;
using Xunit;

namespace DomainTracker.Tests.Business
{
    public class DomainServiceTests
    {
        private readonly Mock<IDomainRepository> _domainRepository = new();
        private readonly Mock<IRdapClient> _rdapClient = new();
        private readonly DomainService _sut;

        public DomainServiceTests()
        {
            _sut = new DomainService(_domainRepository.Object, _rdapClient.Object, TestMapper.Create());
        }

        [Fact]
        public async Task CheckAsync_WhenDomainNameIsInvalid_ReturnsBadRequestWithoutCallingRdap()
        {
            var result = await _sut.CheckAsync("not_a_valid_domain");

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            _rdapClient.Verify(c => c.LookupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _domainRepository.Verify(r => r.AddAsync(It.IsAny<Domain>()), Times.Never);
        }

        [Fact]
        public async Task CheckAsync_WhenDomainIsNew_CreatesRowFromRdapResultAndNormalizesName()
        {
            _domainRepository.Setup(r => r.GetByNameAsync("example.com")).ReturnsAsync((Domain?)null);
            _rdapClient
                .Setup(c => c.LookupAsync("example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RdapLookupResult(false, new DateTime(2030, 1, 1)));

            Domain? added = null;
            _domainRepository
                .Setup(r => r.AddAsync(It.IsAny<Domain>()))
                .Callback<Domain>(d => added = d)
                .Returns(Task.CompletedTask);

            var result = await _sut.CheckAsync("  EXAMPLE.com  ");

            Assert.True(result.Success);
            Assert.Equal("example.com", result.Data!.Name);
            Assert.False(result.Data.IsAvailable);
            Assert.Equal(new DateTime(2030, 1, 1), result.Data.ExpirationDate);
            _domainRepository.Verify(r => r.AddAsync(It.IsAny<Domain>()), Times.Once);
            _domainRepository.Verify(r => r.UpdateAsync(It.IsAny<Domain>()), Times.Never);
            Assert.NotNull(added);
            Assert.Equal("example.com", added!.Name);
        }

        [Fact]
        public async Task CheckAsync_WhenDomainAlreadyExists_UpdatesExistingRowInPlaceInsteadOfInserting()
        {
            var existing = new Domain
            {
                Id = 5,
                Name = "example.com",
                IsAvailable = true,
                LastCheckedAt = DateTime.UtcNow.AddDays(-1),
            };
            _domainRepository.Setup(r => r.GetByNameAsync("example.com")).ReturnsAsync(existing);
            _rdapClient
                .Setup(c => c.LookupAsync("example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RdapLookupResult(false, new DateTime(2031, 1, 1)));

            var result = await _sut.CheckAsync("example.com");

            Assert.Equal(5, result.Data!.Id);
            Assert.False(result.Data.IsAvailable);
            Assert.NotNull(existing.UpdatedAt);
            _domainRepository.Verify(r => r.AddAsync(It.IsAny<Domain>()), Times.Never);
            _domainRepository.Verify(r => r.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task CheckAsync_WhenDomainBecomesAvailableAgain_ClearsExpirationDate()
        {
            var existing = new Domain
            {
                Id = 8,
                Name = "expired-domain.com",
                IsAvailable = false,
                ExpirationDate = new DateTime(2020, 1, 1),
                LastCheckedAt = DateTime.UtcNow.AddDays(-30),
            };
            _domainRepository.Setup(r => r.GetByNameAsync("expired-domain.com")).ReturnsAsync(existing);
            _rdapClient
                .Setup(c => c.LookupAsync("expired-domain.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RdapLookupResult(true, null));

            var result = await _sut.CheckAsync("expired-domain.com");

            Assert.True(result.Data!.IsAvailable);
            Assert.Null(result.Data.ExpirationDate);
        }
    }
}
