using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Client.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly HttpClient _httpClient;

        public SecurityService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("identityServer");
        }

        public async Task<DiscoveryDocumentResponse> GetDisco(string url)
        {
            var disco = await _httpClient.GetDiscoveryDocumentAsync(url);
            return disco;
        }

        public async Task<TokenResponse> GetToken(string tokenEndpoint, string clientId, string clientSecret, string scope)
        {
            // request token
            var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,

                Scope = scope
            });

            return tokenResponse;
        }
    }
}
