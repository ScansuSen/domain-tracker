using AutoMapper;
using DomainTracker.Business.Mapping;
using Microsoft.Extensions.Logging.Abstractions;

namespace DomainTracker.Tests.Business
{
    internal static class TestMapper
    {
        public static IMapper Create()
        {
            var configuration = new MapperConfiguration(
                cfg =>
                {
                    cfg.AddProfile<DomainProfile>();
                    cfg.AddProfile<FavoriteDomainProfile>();
                },
                NullLoggerFactory.Instance);

            return configuration.CreateMapper();
        }
    }
}
