using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GameExplorer.Client;
using MudBlazor.Services;
using GameExplorer.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using GameExplorer.Client.Services.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// in development the web api runs on its own port, read from wwwroot/appsettings.Development.json.
// Once published, web api and client share one address and that file is not read,
// so the address the client was downloaded from is the right one
var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddTransient<CookieHandler>();
builder.Services.AddHttpClient("Account", client => client.BaseAddress = new Uri(apiBaseUrl)).AddHttpMessageHandler<CookieHandler>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CookieAuthenticationStateProvider>());
builder.Services.AddScoped<SearchFocusService>();
var useFakeApi = builder.Configuration.GetValue<bool>("Api:UseFake");
if (useFakeApi)
{
	builder.Services.AddScoped<IGameApiClient, FakeGameApiClient>();
}
else
{
	builder.Services.AddHttpClient<IGameApiClient, GameApiClient>(client =>
	{
		client.BaseAddress = new Uri(apiBaseUrl);
		client.Timeout = TimeSpan.FromSeconds(10);
	}).AddHttpMessageHandler<CookieHandler>();
}

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
// snackbars ("Profilo salvato") at the bottom: top right they covered the user name and the theme button
builder.Services.AddMudServices(config =>
	config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomCenter);

await builder.Build().RunAsync();

