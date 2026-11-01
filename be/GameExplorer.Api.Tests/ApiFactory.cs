using GameExplorer.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace GameExplorer.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
	private HttpMessageHandler _rawg;
	private TimeSpan? _timeout;

	public ApiFactory( HttpMessageHandler rawg, TimeSpan? timeout =  null ) { _rawg = rawg; _timeout = timeout; }

	public const string FakeKey = "fake-key";
	protected override void ConfigureWebHost( IWebHostBuilder builder )
	{
		builder.UseEnvironment("Testing");
		builder.UseSetting("Rawg:ApiKey", FakeKey);
		builder.UseSetting("Cors:FrontendOrigin", "http://localhost:500");
		builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Port=1;Database=not_used;Username=test;Password=test");
		builder.ConfigureServices(services
			=>	{
					services.AddHttpClient<RawgService>()
						.ConfigurePrimaryHttpMessageHandler(() => _rawg )
						.ConfigureHttpClient(c
							=>	{
									if (_timeout is { } t) c.Timeout = t;
								});

				});
	}
}
