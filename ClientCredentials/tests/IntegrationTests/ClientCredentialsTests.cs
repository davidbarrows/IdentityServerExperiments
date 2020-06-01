using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using Quickstart.Tests.Helpers;
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
            _fixture.IdentityServerHttpClient = new HttpClient();
            _fixture.WebApiHttpClient = new HttpClient();
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

        // "additional experiments" from the Quickstart

        [Fact]
        public async Task Given_IdentityServer_Is_Down_Expected_Response_Occurs()
        {
            // Arrange - kill IdentityServer
            ProcManager.KillByPort(_fixture.IdentityServerWrapper.HttpsPort);

            // Act
            var disco = await _fixture.IdentityServerHttpClient.GetDiscoveryDocumentAsync(_fixture.IdentityServerWrapper.UriString);

            // Assert
            disco.IsError.Should().BeTrue();
            disco.Error.StartsWith("Error connecting").Should().BeTrue();

            // restart IdentityServer
            _fixture.RestartIdentityServer();
        }

        [Fact]
        public async Task Given_Bad_ClientId_Is_Sent_Token_Response_Is_Error()
        {
            // Arrange

            // get discovery document
            var disco = await _fixture.IdentityServerHttpClient.GetDiscoveryDocumentAsync(_fixture.IdentityServerWrapper.UriString);
            var tokenEndpoint = disco.TokenEndpoint;

            // Act
            var tokenResponse = await _fixture.IdentityServerHttpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = "client_id_is_bad",
                ClientSecret = "secret",
                Scope = "api1"
            });

            // Assert
            tokenResponse.IsError.Should().BeTrue();
            tokenResponse.Error.Should().Be("invalid_client");
            tokenResponse.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Given_Bad_Client_Secret_Is_Sent_Token_Response_Is_Error()
        {
            // Arrange

            // get discovery document
            var disco = await _fixture.IdentityServerHttpClient.GetDiscoveryDocumentAsync(_fixture.IdentityServerWrapper.UriString);
            var tokenEndpoint = disco.TokenEndpoint;

            // Act
            var tokenResponse = await _fixture.IdentityServerHttpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret_is_bad",
                Scope = "api1"
            });

            // Assert
            tokenResponse.IsError.Should().BeTrue();
            tokenResponse.Error.Should().Be("invalid_client");
            tokenResponse.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Given_Bad_Scope_Is_Sent_Token_Response_Is_Error()
        {
            // Arrange

            // get discovery document
            var disco = await _fixture.IdentityServerHttpClient.GetDiscoveryDocumentAsync(_fixture.IdentityServerWrapper.UriString);
            var tokenEndpoint = disco.TokenEndpoint;

            // Act
            var tokenResponse = await _fixture.IdentityServerHttpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1_bad_scope"
            });

            // Assert
            tokenResponse.IsError.Should().BeTrue();
            tokenResponse.Error.Should().Be("invalid_scope");
            tokenResponse.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Given_Api_Is_Down_Expected_Response_Occurs()
        {
            // Arrange - kill the API
            ProcManager.KillByPort(_fixture.WebApiWrapper.HttpsPort);

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
            await Assert.ThrowsAsync<HttpRequestException>( () => _fixture.WebApiHttpClient.GetAsync($"{_fixture.WebApiWrapper.UriString}/identity"));

            // reset
            _fixture.RestartWebApi();
        }

        [Fact]
        public async Task Given_No_Token_Is_Sent_To_The_Api_Unauthorized_Response_Occurs()
        {
            // Act
            var apiResponse = await _fixture.WebApiHttpClient.GetAsync($"{_fixture.WebApiWrapper.UriString}/identity");

            // Assert
            apiResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
