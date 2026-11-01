using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using System.Net;
using System.Runtime.ExceptionServices;
using Xunit.Abstractions;

namespace GameExplorer.Api.Tests;

public class CacheTests : TestData
{
	// FakeRawg settato per rispondere a ogni chamata con 200 e un json con le informazioni obbligatorie
	// il client fa una chiamata a FakeRawg cercando i details del gioco id = 3498, controlla che lo status code sia 200 e lo salva in una variabile calls.
	// Dopodiche il client esegue un'altra chiamata allo stesso endpoint e compara le chiamate a FakeRawg alla variabile calls 
	[Fact]
	public async Task App_DoesNotCallRawgAtSecondRequest()
	{
		var rawg = FakeRawgHandler.Status(HttpStatusCode.OK, ValidBody);
		await using var app = new ApiFactory(rawg);
		var client = app.CreateClient();

		var first = await client.GetAsync("/api/games/3498");
		Assert.Equal(HttpStatusCode.OK, first.StatusCode);
		int calls = rawg.Calls;

		await client.GetAsync("/api/games/3498");
		Assert.Equal(calls, rawg.Calls);
	}

	[Fact]
	public async Task Cache_DoesNotSaveConnectionErrors()
	{
		var rawg = FakeRawgHandler.Throws(new HttpRequestException("Connection Denied"));
		await using var app = new ApiFactory(rawg);
		var client = app.CreateClient();

		var first = await client.GetAsync("/api/games/3498");
		Assert.Equal(HttpStatusCode.BadGateway, first.StatusCode);
		int calls = rawg.Calls;

		var second = await client.GetAsync("/api/games/3498");
		Assert.Equal(HttpStatusCode.BadGateway, second.StatusCode);
		Assert.True(rawg.Calls > calls);
	}

	// FakeRawg settato per rispondere a ogni chamata con 400
	// il client fa due chiamate a FakeRawg, cals deve clntare entrambe le chiamate
	[Theory]
	[MemberData(nameof(EndpointsPerCode))]
	public async Task Cache_DoesNotSaveRawgErrors( string url, HttpStatusCode errorCode )
	{
		var rawg = FakeRawgHandler.Status(errorCode, ErrorBody);
		await using var app = new ApiFactory(rawg);
		var client = app.CreateClient();

		var first = await client.GetAsync(url);
		Assert.Equal(HttpStatusCode.BadGateway, first.StatusCode);
		int calls = rawg.Calls;

		var second = await client.GetAsync(url);
		Assert.True(rawg.Calls > calls);
	}

	[Theory]
	[MemberData(nameof(EndpointsPerCode))]
	public async Task Cache_RecoversWhenRawgRecovers( string url, HttpStatusCode errorCode )
	{
		bool failing = true;
		var rawg = new FakeRawgHandler((_,_,_) =>
			Task.FromResult(failing
				? FakeRawgHandler.Json(errorCode, ErrorBody)
				: FakeRawgHandler.Json(HttpStatusCode.OK, ValidBody)));
		await using var app = new ApiFactory(rawg);
		var client = app.CreateClient();

		var first = await client.GetAsync(url);
		Assert.Equal(HttpStatusCode.BadGateway, first.StatusCode);
		
		failing = false;
		var second = await client.GetAsync(url);
		Assert.Equal(HttpStatusCode.OK, second.StatusCode);
		int calls = rawg.Calls;

		var third = await client.GetAsync(url);
		Assert.Equal(HttpStatusCode.OK, third.StatusCode);
		Assert.Equal(calls, rawg.Calls);
	}

	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task Cache_DoesNotSaveBrokenJson( string url )
	{
		var rawg = FakeRawgHandler.Status(HttpStatusCode.OK, BrokenBody);
		await using var app = new ApiFactory(rawg);
		var client = app.CreateClient();

		var first = await client.GetAsync(url);
		Assert.Equal(HttpStatusCode.BadGateway, first.StatusCode);
		int calls = rawg.Calls;

		var second = await client.GetAsync(url);
		Assert.Equal(HttpStatusCode.BadGateway, second.StatusCode);
		Assert.True(rawg.Calls > calls);
	}
}
