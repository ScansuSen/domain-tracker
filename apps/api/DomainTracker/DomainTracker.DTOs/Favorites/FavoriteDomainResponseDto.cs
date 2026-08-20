namespace DomainTracker.DTOs.Favorites
{
    public class FavoriteDomainResponseDto
    {
        public FavoriteDomainResponseDto(int id, string domainName, bool isAvailable, DateTime lastCheckedAt, DateTime? expirationDate, DateTime createdAt)
        {
            Id = id;
            DomainName = domainName;
            IsAvailable = isAvailable;
            LastCheckedAt = lastCheckedAt;
            ExpirationDate = expirationDate;
            CreatedAt = createdAt;
        }

        public int Id { get; }

        public string DomainName { get; }

        public bool IsAvailable { get; }

        public DateTime LastCheckedAt { get; }

        public DateTime? ExpirationDate { get; }

        public DateTime CreatedAt { get; }
    }
}
