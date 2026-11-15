using Bunit;
using Bunit.TestDoubles;
using GameExplorer.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace GameExplorer.Client.Tests;

// base of every page test: bUnit + MudBlazor + fake client, set up once instead of in every test.
// The context is a field and not the class to inherit from: this way xUnit does not call the
// synchronous Dispose(), which fails with MudBlazor services (they only close asynchronously)
public abstract class PageTestBase : IAsyncLifetime
{
	protected BunitContext Ctx { get; } = new();
	// the same object the page receives, so a test can change it (e.g. SimulateError = true)
	protected FakeGameApiClient Api { get; } = new();
	protected BunitAuthorizationContext Auth { get; }
	protected PageTestBase()
	{
		Auth = Ctx.AddAuthorization();
		// the fake client answers after 800ms: the default 1 second wait is too short and tests failed at random
		BunitContext.DefaultWaitTimeout = TimeSpan.FromSeconds(5);
		Ctx.Services.AddMudServices();
		Ctx.Services.AddSingleton<IGameApiClient>(Api);
		Ctx.Services.AddScoped<SearchFocusService>();
		// no browser here: MudBlazor JS calls get an empty answer instead of failing the test
		Ctx.JSInterop.Mode = JSRuntimeMode.Loose;
	}
	public Task InitializeAsync() => Task.CompletedTask;
	public async Task DisposeAsync() => await Ctx.DisposeAsync();
}
