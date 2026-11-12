using Bunit;
using Bunit.TestDoubles;              // esiste già (pacchetto bunit): BunitAuthorizationContext
using GameExplorer.Client.Services;   // esiste già: IGameApiClient, FakeGameApiClient
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;             // esiste già (pacchetto MudBlazor): AddMudServices

namespace GameExplorer.Client.Tests;

// Base di tutti i test di pagina: bUnit + MudBlazor + client finto.
// Il contesto è un campo e non la classe da cui ereditare: così xUnit non chiama il
// Dispose() sincrono, che fallisce con i servizi di MudBlazor (si chiudono solo in modo asincrono).
public abstract class PageTestBase : IAsyncLifetime
{
	protected BunitContext Ctx { get; } = new();
	// Lo stesso oggetto che riceve la pagina: il test può cambiarlo (es. SimulateError = true)
	protected FakeGameApiClient Api { get; } = new();
	protected BunitAuthorizationContext Auth { get; }
	protected PageTestBase()
	{
		Auth = Ctx.AddAuthorization();
		// Il client finto risponde dopo 800 ms: 1 secondo di attesa (il valore di base) è troppo poco
		BunitContext.DefaultWaitTimeout = TimeSpan.FromSeconds(5);
		Ctx.Services.AddMudServices();
		Ctx.Services.AddSingleton<IGameApiClient>(Api);
		Ctx.Services.AddScoped<SearchFocusService>();
		// Nessun browser: le chiamate JS di MudBlazor ricevono una risposta vuota invece di un errore
		Ctx.JSInterop.Mode = JSRuntimeMode.Loose;
	}
	public Task InitializeAsync() => Task.CompletedTask;
	public async Task DisposeAsync() => await Ctx.DisposeAsync();
}
