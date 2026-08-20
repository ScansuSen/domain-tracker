namespace DomainTracker.Business.Abstract
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTime ExpiresAtUtc) GenerateToken(int userId, string username);
    }
}
