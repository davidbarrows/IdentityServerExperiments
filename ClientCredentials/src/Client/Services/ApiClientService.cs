using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Client.Services
{
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _httpClient;

        public ApiClientService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("api");
        }

        public async Task<HttpResponseMessage> GetAsync(string url, string accessToken)
        {
            _httpClient.SetBearerToken(accessToken);
            var response = await _httpClient.GetAsync(url);

            return response;
        }
    }
}
