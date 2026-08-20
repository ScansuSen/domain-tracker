namespace DomainTracker.DTOs.Domains
{
    public class DomainResponseDto
    {
        public DomainResponseDto(int id, string name, bool isAvailable, DateTime lastCheckedAt, DateTime? expirationDate)
        {
            Id = id;
            Name = name;
            IsAvailable = isAvailable;
            LastCheckedAt = lastCheckedAt;
            ExpirationDate = expirationDate;
        }

        public int Id { get; }

        public string Name { get; }

        public bool IsAvailable { get; }

        public DateTime LastCheckedAt { get; }

        public DateTime? ExpirationDate { get; }
    }
}
