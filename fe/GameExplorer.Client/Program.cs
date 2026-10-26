using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GameExplorer.Client;
using MudBlazor.Services;
using GameExplorer.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? throw new InvalidOperationException("Manca 'Api:BaseUrl' in wwwroot/appsettings.json: inserisci l'indirizzo della Web API, ad es. 'http://localhost:5290/'.");

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// per passare alla modalità realistica scommenta la funzione lambda
// e commenta la riga dopo il commento
// builder.Services.AddHttpClient<IGameApiClient, GameApiClient>(client =>
// {
// 	client.BaseAddress = new Uri(apiBaseUrl);
// });
builder.Services.AddScoped<IGameApiClient, FakeGameApiClient>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();

