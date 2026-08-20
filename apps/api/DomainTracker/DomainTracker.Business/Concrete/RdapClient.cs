using System.Net;
using System.Net.Http.Json;
using DomainTracker.Business.Abstract;
using DomainTracker.Business.Models;
using DomainTracker.Core.Constants;
using DomainTracker.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace DomainTracker.Business.Concrete
{
    public class RdapClient : IRdapClient
    {
        private const string ExpirationEventAction = "expiration";

        private readonly HttpClient _httpClient;
        private readonly ILogger<RdapClient> _logger;

        public RdapClient(HttpClient httpClient, ILogger<RdapClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<RdapLookupResult> LookupAsync(string domainName, CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync($"domain/{Uri.EscapeDataString(domainName)}/", cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(ex, "RDAP lookup failed for {DomainName}", domainName);
                throw new ExternalServiceException(Messages.RdapServiceUnreachable);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new RdapLookupResult(IsAvailable: true, ExpirationDate: null);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "RDAP lookup for {DomainName} returned unexpected status {StatusCode}",
                    domainName,
                    (int)response.StatusCode);
                throw new ExternalServiceException(Messages.RdapServiceUnreachable);
            }

            RdapResponseModel? payload;
            try
            {
                payload = await response.Content.ReadFromJsonAsync<RdapResponseModel>(cancellationToken);
            }
            catch (Exception ex) when (ex is System.Text.Json.JsonException or NotSupportedException)
            {
                _logger.LogError(ex, "RDAP response for {DomainName} could not be parsed as JSON", domainName);
                throw new ExternalServiceException(Messages.RdapServiceUnreachable);
            }

            var expirationDate = payload?.Events?
                .FirstOrDefault(e => string.Equals(e.EventAction, ExpirationEventAction, StringComparison.OrdinalIgnoreCase))
                ?.EventDate;

            return new RdapLookupResult(IsAvailable: false, ExpirationDate: expirationDate);
        }
    }
}
