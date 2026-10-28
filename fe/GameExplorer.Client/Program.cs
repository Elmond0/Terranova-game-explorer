using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GameExplorer.Client;
using MudBlazor.Services;
using GameExplorer.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// In sviluppo l'indirizzo della Web API arriva da wwwroot/appsettings.Development.json.
// Nel sito pubblicato quel file non viene letto: Web API e client stanno sullo stesso
// indirizzo, quindi si usa quello da cui è stato scaricato il client.
var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? builder.HostEnvironment.BaseAddress;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<CookieHandler>();
// Client verso la Web API.
// Finché gli endpoint della Home (top-rated, recent, upcoming) non sono su main si usa il Fake.
// Per usare la Web API vera: scommentare il blocco AddHttpClient e commentare la riga AddScoped.
builder.Services.AddHttpClient<IGameApiClient, GameApiClient>(client =>
{
	client.BaseAddress = new Uri(apiBaseUrl);
	client.Timeout = TimeSpan.FromSeconds(10);
}).AddHttpMessageHandler<CookieHandler>();
builder.Services.AddScoped<IGameApiClient, FakeGameApiClient>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();

