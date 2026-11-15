using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GameExplorer.Api.Tests;

public class IgdbConfigTests
{
    [Theory]
    [InlineData("ClientId")]
    [InlineData("ClientSecret")]
    public void App_does_not_start_without_Igdb_credentials( string missing )
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.UseSetting("Igdb:ClientId", missing == "ClientId" ? "" : "fake-client-id");
                builder.UseSetting("Igdb:ClientSecret", missing == "ClientSecret" ? "" : "fake-client-secret");
                builder.UseSetting("Cors:FrontendOrigin", "http://localhost:5100");
                builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Port=1;Database=not_used;Username=test;Password=test");
            });
        
        var ex = Assert.ThrowsAny<Exception>(() => factory.CreateClient());
        
        Assert.Contains(missing, ex.ToString());
    }
}