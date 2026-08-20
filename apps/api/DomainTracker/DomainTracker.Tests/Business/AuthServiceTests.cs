using DomainTracker.Business.Abstract;
using DomainTracker.Business.Concrete;
using DomainTracker.DataAccess.Abstract;
using DomainTracker.DTOs.Auth;
using DomainTracker.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace DomainTracker.Tests.Business
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IPasswordHasher<User>> _passwordHasher = new();
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _sut = new AuthService(_userRepository.Object, _passwordHasher.Object, _jwtTokenGenerator.Object);
        }

        [Fact]
        public async Task RegisterAsync_WhenUsernameIsInvalid_ReturnsBadRequestWithoutPersisting()
        {
            var request = new RegisterRequestDto { Username = "ab", Password = "Passw0rd!" };

            var result = await _sut.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains(result.Messages, m => m.StartsWith("Username", StringComparison.Ordinal));
            _userRepository.Verify(r => r.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
            _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WhenUsernameAlreadyTaken_ReturnsConflictResultWithoutPersisting()
        {
            _userRepository
                .Setup(r => r.GetByUsernameAsync("alice"))
                .ReturnsAsync(new User { Id = 1, Username = "alice" });

            var request = new RegisterRequestDto { Username = "alice", Password = "Passw0rd!" };

            var result = await _sut.RegisterAsync(request);

            Assert.False(result.Success);
            Assert.Equal(409, result.StatusCode);
            _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WhenUsernameFree_HashesPasswordPersistsUserAndReturnsToken()
        {
            _userRepository.Setup(r => r.GetByUsernameAsync("newuser")).ReturnsAsync((User?)null);
            _passwordHasher.Setup(h => h.HashPassword(It.IsAny<User>(), "Passw0rd!")).Returns("hashed-password");
            _jwtTokenGenerator
                .Setup(g => g.GenerateToken(It.IsAny<int>(), "newuser"))
                .Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));

            var request = new RegisterRequestDto { Username = "newuser", Password = "Passw0rd!" };

            var result = await _sut.RegisterAsync(request);

            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            _userRepository.Verify(
                r => r.AddAsync(It.Is<User>(u => u.Username == "newuser" && u.PasswordHash == "hashed-password")),
                Times.Once);
            Assert.Equal("jwt-token", result.Data!.Token);
            Assert.Equal("newuser", result.Data.Username);
        }

        [Fact]
        public async Task LoginAsync_WhenUsernameIsEmpty_ReturnsBadRequestWithoutCallingRepository()
        {
            var request = new LoginRequestDto { Username = "", Password = "whatever" };

            var result = await _sut.LoginAsync(request);

            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            _userRepository.Verify(r => r.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WhenUserDoesNotExist_ReturnsUnauthorizedResult()
        {
            _userRepository.Setup(r => r.GetByUsernameAsync("ghost")).ReturnsAsync((User?)null);

            var request = new LoginRequestDto { Username = "ghost", Password = "whatever" };

            var result = await _sut.LoginAsync(request);

            Assert.False(result.Success);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsWrong_ReturnsUnauthorizedResult()
        {
            var user = new User { Id = 2, Username = "alice", PasswordHash = "hashed" };
            _userRepository.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync(user);
            _passwordHasher
                .Setup(h => h.VerifyHashedPassword(user, "hashed", "wrong-password"))
                .Returns(PasswordVerificationResult.Failed);

            var request = new LoginRequestDto { Username = "alice", Password = "wrong-password" };

            var result = await _sut.LoginAsync(request);

            Assert.False(result.Success);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_WhenCredentialsAreValid_ReturnsSuccessResultWithToken()
        {
            var user = new User { Id = 3, Username = "alice", PasswordHash = "hashed" };
            _userRepository.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync(user);
            _passwordHasher
                .Setup(h => h.VerifyHashedPassword(user, "hashed", "correct-password"))
                .Returns(PasswordVerificationResult.Success);
            _jwtTokenGenerator
                .Setup(g => g.GenerateToken(3, "alice"))
                .Returns(("jwt-token", DateTime.UtcNow.AddHours(1)));

            var request = new LoginRequestDto { Username = "alice", Password = "correct-password" };

            var result = await _sut.LoginAsync(request);

            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("jwt-token", result.Data!.Token);
            Assert.Equal("alice", result.Data.Username);
        }
    }
}
