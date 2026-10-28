using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace GameExplorer.Api.Tests;

public class CorsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string AllowedOrigin = "http://localhost:5100";
    private const string OtherOrigin = "http://localhost:9999";

    private readonly WebApplicationFactory<Program> _factory;

    public CorsTests(WebApplicationFactory<Program> factory)
    {
        // Forza l'origine consentita, così il test non dipende da appsettings.Development.json
        _factory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Cors:FrontendOrigin", AllowedOrigin));
    }

    private static HttpRequestMessage Preflight(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        return request;
    }

    [Fact]
    public async Task Preflight_from_frontend_origin_is_allowed()
    {
        var client = _factory.CreateClient();

        var response = await client.SendAsync(Preflight(AllowedOrigin));

        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));
        Assert.Equal(AllowedOrigin, Assert.Single(values));

        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Credentials", out var creds));
        Assert.Equal("true", Assert.Single(creds));
    }

    [Fact]
    public async Task Preflight_from_other_origin_is_not_allowed()
    {
        var client = _factory.CreateClient();

        var response = await client.SendAsync(Preflight(OtherOrigin));

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }
}