using AutoMapper;
using DomainTracker.Business.Abstract;
using DomainTracker.Core.Constants;
using DomainTracker.Core.Results;
using DomainTracker.DataAccess.Abstract;
using DomainTracker.DataAccess.Enums;
using DomainTracker.DataAccess.Models;
using DomainTracker.DTOs.Favorites;
using DomainTracker.Entities.Models;

namespace DomainTracker.Business.Concrete
{
    public class FavoriteDomainService : IFavoriteDomainService
    {
        private readonly IFavoriteDomainRepository _favoriteDomainRepository;
        private readonly IDomainService _domainService;
        private readonly IMapper _mapper;

        public FavoriteDomainService(
            IFavoriteDomainRepository favoriteDomainRepository,
            IDomainService domainService,
            IMapper mapper)
        {
            _favoriteDomainRepository = favoriteDomainRepository;
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

            var checkedDomain = checkResult.Data!;

            var checkInfo = new DomainCheckInfo(checkedDomain.IsAvailable, checkedDomain.LastCheckedAt, checkedDomain.ExpirationDate);
            var (outcome, favorite) = await _favoriteDomainRepository.AddFavoriteAsync(userId, checkedDomain.Name, checkInfo);
            if (outcome == AddFavoriteOutcome.AlreadyFavorited)
                return new ErrorDataResult<FavoriteDomainResponseDto>(HttpStatusCodes.Conflict, Messages.DomainAlreadyInFavorites(checkedDomain.Name));

            return new SuccessDataResult<FavoriteDomainResponseDto>(_mapper.Map<FavoriteDomainResponseDto>(favorite!), HttpStatusCodes.Created, Messages.DomainAddedToFavorites);
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

            var checkResult = await _domainService.CheckAsync(favorite.Domain.Name);
            if (!checkResult.Success)
                return new ErrorDataResult<FavoriteDomainResponseDto>(checkResult.StatusCode, checkResult.Messages);

            var refreshed = checkResult.Data!;
            var checkInfo = new DomainCheckInfo(refreshed.IsAvailable, refreshed.LastCheckedAt, refreshed.ExpirationDate);
            await _favoriteDomainRepository.RefreshDomainAsync(favorite, checkInfo);

            return new SuccessDataResult<FavoriteDomainResponseDto>(_mapper.Map<FavoriteDomainResponseDto>(favorite), HttpStatusCodes.Ok, Messages.FavoriteRefreshed);
        }
    }
}
