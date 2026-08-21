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
        }

        [Fact]
        public async Task CheckAsync_NormalizesNameAndReturnsFreshRdapResultWithoutPersisting()
        {
            _rdapClient
                .Setup(c => c.LookupAsync("example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RdapLookupResult(false, new DateTime(2030, 1, 1)));

            var result = await _sut.CheckAsync("  EXAMPLE.com  ");

            Assert.True(result.Success);
            Assert.Equal("example.com", result.Data!.Name);
            Assert.Equal(0, result.Data.Id);
            Assert.False(result.Data.IsAvailable);
            Assert.Equal(new DateTime(2030, 1, 1), result.Data.ExpirationDate);
            _domainRepository.Verify(r => r.GetByNameAsync(It.IsAny<string>()), Times.Never);
            _domainRepository.Verify(r => r.AddAsync(It.IsAny<Domain>()), Times.Never);
            _domainRepository.Verify(r => r.UpdateAsync(It.IsAny<Domain>()), Times.Never);
        }

        [Fact]
        public async Task CheckAsync_WhenDomainIsAvailable_ReturnsNullExpirationDate()
        {
            _rdapClient
                .Setup(c => c.LookupAsync("expired-domain.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RdapLookupResult(true, null));

            var result = await _sut.CheckAsync("expired-domain.com");

            Assert.True(result.Data!.IsAvailable);
            Assert.Null(result.Data.ExpirationDate);
        }
    }
}
