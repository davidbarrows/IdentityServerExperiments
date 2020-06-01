using System.Net.Http;
using System.Threading.Tasks;

namespace Client.Services
{
    public interface IApiClientService
    {
        Task<HttpResponseMessage> GetAsync(string url, string accessToken);
    }
}
