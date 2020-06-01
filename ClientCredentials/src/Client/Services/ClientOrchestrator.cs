using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client.Services
{
    public class ClientOrchestrator
    {
        private readonly ISecurityService _securityService;
        private readonly IApiClientService _apiClientService;
        private readonly ILogger _logger;

        public ClientOrchestrator(ISecurityService securityService, IApiClientService apiClientService, ILogger logger)
        {
            _securityService = securityService;
            _apiClientService = apiClientService;
            _logger = logger;
        }

        public async Task Execute()
        {
            // discover endpoints from metadata
            var disco = await _securityService.GetDisco("https://localhost:5001");
            if (disco.IsError)
            {
                _logger.LogError(disco.Error);
                return;
            }

            // request token
            var tokenResponse = await _securityService.GetToken(disco.TokenEndpoint, "client", "secret", "api1");
            if (tokenResponse.IsError)
            {
                _logger.LogError(tokenResponse.Error);
                return;
            }

            _logger.LogInformation(tokenResponse.Json.ToString());
            _logger.LogInformation("\n\n");

            // call api
            var apiResponse = await _apiClientService.GetAsync("https://localhost:6001/identity", tokenResponse.AccessToken);
            if (!apiResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation(apiResponse.StatusCode.ToString());
            }
            else
            {
                var content = await apiResponse.Content.ReadAsStringAsync();
                _logger.LogInformation(JArray.Parse(content).ToString());
            }
        }
    }
}
