using System.Threading.Tasks;
using IdentityModel.Client;

namespace Client.Services
{
    public interface ISecurityService
    {
        Task<DiscoveryDocumentResponse> GetDisco(string url);

        Task<TokenResponse> GetToken(string tokenEndpoint, string clientId, string clientSecret, string scope);
    }
}
