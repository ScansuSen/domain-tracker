using AutoMapper;
using DomainTracker.Business.Abstract;
using DomainTracker.Core.Constants;
using DomainTracker.Core.Results;
using DomainTracker.Core.Utilities;
using DomainTracker.DataAccess.Abstract;
using DomainTracker.DTOs.Favorites;
using DomainTracker.Entities.Models;

namespace DomainTracker.Business.Concrete
{
    public class FavoriteDomainService : IFavoriteDomainService
    {
        private readonly IFavoriteDomainRepository _favoriteDomainRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly IDomainService _domainService;
        private readonly IMapper _mapper;

        public FavoriteDomainService(
            IFavoriteDomainRepository favoriteDomainRepository,
            IDomainRepository domainRepository,
            IDomainService domainService,
            IMapper mapper)
        {
            _favoriteDomainRepository = favoriteDomainRepository;
            _domainRepository = domainRepository;
            _domainService = domainService;
            _mapper = mapper;
        }

        public async Task<IDataResult<List<FavoriteDomainResponseDto>>> GetAllForUserAsync(int userId)
        {
            var favorites = await _favoriteDomainRepository.GetByUserIdWithDomainAsync(userId);
            var dtos = _mapper.Map<List<FavoriteDomainResponseDto>>(favorites);
            return new SuccessDataResult<List<FavoriteDomainResponseDto>>(dtos);
        }

        public async Task<IDataResult<FavoriteDomainResponseDto>> AddAsync(int userId, string domainName)
        {
            var checkResult = await _domainService.CheckAsync(domainName);
            if (!checkResult.Success)
                return new ErrorDataResult<FavoriteDomainResponseDto>(checkResult.StatusCode, checkResult.Messages);

            var normalizedName = DomainNameNormalizer.Normalize(domainName);
            var domain = await _domainRepository.GetByNameAsync(normalizedName);
            if (domain is null)
                throw new InvalidOperationException(Messages.DomainNotPersistedAfterCheck(normalizedName));//throw because unexpected error

            if (await _favoriteDomainRepository.ExistsAsync(userId, domain.Id))
                return new ErrorDataResult<FavoriteDomainResponseDto>(HttpStatusCodes.Conflict, Messages.DomainAlreadyInFavorites(normalizedName));

            var favorite = new FavoriteDomain { UserId = userId, DomainId = domain.Id };
            await _favoriteDomainRepository.AddAsync(favorite);

            favorite.Domain = domain;
            return new SuccessDataResult<FavoriteDomainResponseDto>(_mapper.Map<FavoriteDomainResponseDto>(favorite), HttpStatusCodes.Created, Messages.DomainAddedToFavorites);
        }

        public async Task<IResult> DeleteAsync(int userId, int favoriteId)
        {
            var favorite = await _favoriteDomainRepository.GetByIdAsync(favoriteId);

            if (favorite is null || favorite.UserId != userId)
                return new ErrorResult(HttpStatusCodes.NotFound, Messages.FavoriteNotFound(favoriteId));

            await _favoriteDomainRepository.DeleteAsync(favorite);
            return new SuccessResult(HttpStatusCodes.Ok, Messages.FavoriteRemoved);
        }

        public async Task<IDataResult<FavoriteDomainResponseDto>> RefreshAsync(int userId, int favoriteId)
        {
            var favorite = await _favoriteDomainRepository.GetByIdWithDomainAsync(favoriteId);
            if (favorite is null || favorite.UserId != userId)
                return new ErrorDataResult<FavoriteDomainResponseDto>(HttpStatusCodes.NotFound, Messages.FavoriteNotFound(favoriteId));

            var refreshed = (await _domainService.CheckAsync(favorite.Domain.Name)).Data!;
            favorite.Domain.IsAvailable = refreshed.IsAvailable;
            favorite.Domain.LastCheckedAt = refreshed.LastCheckedAt;
            favorite.Domain.ExpirationDate = refreshed.ExpirationDate;

            return new SuccessDataResult<FavoriteDomainResponseDto>(_mapper.Map<FavoriteDomainResponseDto>(favorite), HttpStatusCodes.Ok, Messages.FavoriteRefreshed);
        }
    }
}
