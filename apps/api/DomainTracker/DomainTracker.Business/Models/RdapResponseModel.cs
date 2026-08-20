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

        [JsonPropertyName("eventDate")]
        public DateTime? EventDate { get; set; }
    }
}
