using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GameExplorer.Api.Tests;

public class RawgConfigTests
{
    [Fact]
    public void App_does_not_start_without_Rawg_ApiKey()
    {
        // Sovrascrive qualsiasi valore da appsettings / user secrets con una chiave vuota
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Rawg:ApiKey", "");
                builder.UseSetting("Cors:FrontendOrigin", "http://localhost:5100");
            });

        // CreateClient avvia l'app: se la configurazione manca, deve lanciare
        var ex = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        // ToString() include anche le eventuali inner exception
        Assert.Contains("ApiKey", ex.ToString());
    }
}