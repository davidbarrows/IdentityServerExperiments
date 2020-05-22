using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Quickstart.Tests.IntegrationTests
{
    public class ClientCredentialsTests : IClassFixture<ClientCredentialsFixture>
    {
        // see https://xunit.net/docs/shared-context
        private readonly ClientCredentialsFixture _fixture;
        public ClientCredentialsTests(ClientCredentialsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Given_IdentityServer_Is_Listening_Should_Get_Disco_And_TokenResponse()
        {
            // Arrange

            // get discovery document
            var disco = await _fixture.IdentityServerHttpClient.GetDiscoveryDocumentAsync(_fixture.IdentityServerWrapper.UriString);
            var tokenEndpoint = disco.TokenEndpoint;

            // Act

            // request token
            var tokenResponse = await _fixture.IdentityServerHttpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });

            // Assert
            disco.Should().NotBeNull();
            tokenResponse.Should().NotBeNull();
        }

        [Fact]
        public async Task Given_IdentityServer_And_Web_Api_Are_Listening_Should_Execute_End_To_End_Happy_Path()
        {
            // Arrange

            // get discovery document
            var disco = await _fixture.IdentityServerHttpClient.GetDiscoveryDocumentAsync(_fixture.IdentityServerWrapper.UriString);
            var tokenEndpoint = disco.TokenEndpoint;

            // request token
            var tokenResponse = await _fixture.IdentityServerHttpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });

            // set bearer token on API client
            _fixture.WebApiHttpClient.SetBearerToken(tokenResponse.AccessToken);

            // Act

            // call api
            var apiResponse = await _fixture.WebApiHttpClient.GetAsync($"{_fixture.WebApiWrapper.UriString}/identity");
            var content = await apiResponse.Content.ReadAsStringAsync();
            var parsedContent = JArray.Parse(content);

            // Assert
            disco.Should().NotBeNull();
            tokenResponse.Should().NotBeNull();
            apiResponse.Should().NotBeNull();
            apiResponse.IsSuccessStatusCode.Should().BeTrue();

            content.Should().BeOfType<string>();
            parsedContent.Should().BeOfType<JArray>();
        }
    }
}
