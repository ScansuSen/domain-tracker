using DomainTracker.Business.Abstract;
using DomainTracker.Business.Validation;
using DomainTracker.Core.Constants;
using DomainTracker.Core.Results;
using DomainTracker.DataAccess.Abstract;
using DomainTracker.DTOs.Auth;
using DomainTracker.Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace DomainTracker.Business.Concrete
{
    public class AuthService : IAuthService
    {
        private static readonly RegisterRequestValidator RegisterValidator = new();
        private static readonly LoginRequestValidator LoginValidator = new();

        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<IDataResult<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            var validationResult = RegisterValidator.Validate(request);
            if (!validationResult.IsValid)
                return new ErrorDataResult<AuthResponseDto>(HttpStatusCodes.BadRequest, validationResult.ToMessages());

            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser is not null)
                return new ErrorDataResult<AuthResponseDto>(HttpStatusCodes.Conflict, Messages.UsernameAlreadyTaken(request.Username));

            var user = new User
            {
                Username = request.Username,
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.AddAsync(user);

            return new SuccessDataResult<AuthResponseDto>(BuildAuthResponse(user), HttpStatusCodes.Created, Messages.UserRegisteredSuccessfully);
        }

        public async Task<IDataResult<AuthResponseDto>> LoginAsync(LoginRequestDto request)
        {
            var validationResult = LoginValidator.Validate(request);
            if (!validationResult.IsValid)
                return new ErrorDataResult<AuthResponseDto>(HttpStatusCodes.BadRequest, validationResult.ToMessages());

            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user is null)
                return new ErrorDataResult<AuthResponseDto>(HttpStatusCodes.Unauthorized, Messages.InvalidCredentials);

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
                return new ErrorDataResult<AuthResponseDto>(HttpStatusCodes.Unauthorized, Messages.InvalidCredentials);

            return new SuccessDataResult<AuthResponseDto>(BuildAuthResponse(user), HttpStatusCodes.Ok, Messages.LoginSuccessful);
        }

        private AuthResponseDto BuildAuthResponse(User user)
        {
            var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user.Id, user.Username);
            return new AuthResponseDto(token, user.Username, expiresAtUtc);
        }
    }
}
