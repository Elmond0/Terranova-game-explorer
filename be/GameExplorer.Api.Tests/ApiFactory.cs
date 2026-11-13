using System.Net;
using GameExplorer.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;

namespace GameExplorer.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
	public const string FakeClientId = "fake-client-id";
	public const string FakeClientSecret = "fake-client-secret";
	public const string FakeToken = "fake-twitch-token";

	private const string TokenBody =
		"""{"access_token":"fake-twitch-token","expires_in":5000000,"token_type":"bearer"}""";

	private readonly HttpMessageHandler _igdb;
	private readonly TimeSpan? _timeout;

	// Finto id.twitch.tv: l'IgdbAuthHandler chiede il token prima di ogni chiamata a IGDB,
	// senza questo finto la suite andrebbe sulla rete vera.
	public FakeIgdbHandler Twitch { get; }

	public ApiFactory( HttpMessageHandler igdb, TimeSpan? timeout = null, FakeIgdbHandler? twitch = null )
	{
		_igdb = igdb;
		_timeout = timeout;
		Twitch = twitch ?? new FakeIgdbHandler((_, _, _) =>
			Task.FromResult(FakeIgdbHandler.Json(HttpStatusCode.OK, TokenBody)));
	}

	protected override void ConfigureWebHost( IWebHostBuilder builder )
	{
		builder.UseEnvironment("Testing");

		// Diagnostica: IgdbExceptionHandler nasconde il motivo del 502, il log lo mostra
		builder.ConfigureLogging(l => l.ClearProviders().AddSimpleConsole().SetMinimumLevel(LogLevel.Warning));
		// Sorgente aggiunta per ultima: vince su appsettings*.json e user secrets
		builder.ConfigureAppConfiguration((_, config) =>
			config.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Igdb:ClientId"] = FakeClientId,
				["Igdb:ClientSecret"] = FakeClientSecret,
			}));
		builder.UseSetting("Cors:FrontendOrigin", "http://localhost:500");
		builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Port=1;Database=not_used;Username=test;Password=test");

		builder.ConfigureTestServices(services =>
		{
			// Sostituiscono solo l'handler primario: IgdbAuthHandler e resilienza restano quelli veri
			services.AddHttpClient<TwitchTokenProvider>()
				.ConfigurePrimaryHttpMessageHandler(() => Twitch);

			services.AddHttpClient<IgdbClient>()
				.ConfigurePrimaryHttpMessageHandler(() => _igdb)
				.ConfigureHttpClient(c =>
				{
					if (_timeout is { } t) c.Timeout = t;
				});

			// Nome della pipeline = "<nome client>-standard", il client tipizzato si chiama IgdbClient
			services.PostConfigure<HttpStandardResilienceOptions>("IgdbClient-standard", o =>
			{
				o.Retry.Delay = TimeSpan.Zero;
				o.AttemptTimeout.Timeout = TimeSpan.FromMilliseconds(200);
			});
		});
	}
}