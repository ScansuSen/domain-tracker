using System.Text.Json.Serialization;

namespace DomainTracker.Business.Models
{
    internal class RdapResponseModel
    {
        [JsonPropertyName("events")]
        public List<RdapEventModel>? Events { get; set; }
    }

    internal class RdapEventModel
    {
        [JsonPropertyName("eventAction")]
        public string? EventAction { get; set; }

        // Used DateTimeOffset to preserve the timezone information from RDAP.
        [JsonPropertyName("eventDate")]
        public DateTimeOffset? EventDate { get; set; }
    }
}
