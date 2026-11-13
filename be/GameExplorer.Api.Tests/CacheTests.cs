using System.Net;
using Xunit;

namespace GameExplorer.Api.Tests;

public class CacheTests : TestData
{
	private const string SearchSkip =
		"La search non e' ancora migrata a IGDB (GamesController.Search e GameService.SearchAsync sono commentati)";

	private const string SearchUrl =
		"/api/games?query=zelda&toYear=2024&ordering=-rating&page=1&pageSize=20";

	private const string OtherSearchUrl =
		"/api/games?query=zelda&genreIds=4&genreIds=3&platformIds=7&fromYear=2016&toYear=2024&ordering=-rating&page=1&pageSize=30";
	
	[Fact]
	public async Task App_DoesNotCallIgdbAtSecondRequest()
	{
		var igdb = FakeIgdbHandler.Status(HttpStatusCode.OK, ValidBody);
		await using var app = new ApiFactory(igdb);
		var client = app.CreateClient();

		var first = await client.GetAsync("/api/games/3498");
		Assert.Equal(HttpStatusCode.OK, first.StatusCode);
		int calls = igdb.Calls;

		await client.GetAsync("/api/games/3498");
		Assert.Equal(calls, igdb.Calls);
	}

	[Fact]
	public async Task App_DoesCallIgdbForDifferentGameId()
	{
		var igdb = FakeIgdbHandler.Status(HttpStatusCode.OK, ValidBody);
		await using var app = new ApiFactory(igdb);
		var client = app.CreateClient();

		Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/games/3498")).StatusCode);
		int calls = igdb.Calls;

		Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/games/1020")).StatusCode);
		Assert.True(igdb.Calls > calls);
	}

	[Fact]
	public async Task App_DoesNotCallIgdbAtSecondRequestSearch()
	{
		var igdb = FakeIgdbHandler.Status(HttpStatusCode.OK, ValidBody);
		await using var app = new ApiFactory(igdb);
		var client = app.CreateClient();

		var first = await client.GetAsync(SearchUrl);
		Assert.Equal(HttpStatusCode.OK, first.StatusCode);
		int calls = igdb.Calls;

		await client.GetAsync(SearchUrl);
		Assert.Equal(calls, igdb.Calls);
	}

	[Fact]
	public async Task App_DoesCallIgdbAtSecondRequestSearchIfFilterIsDifferent()
	{
		var igdb = FakeIgdbHandler.Status(HttpStatusCode.OK, ValidBody);
		await using var app = new ApiFactory(igdb);
		var client = app.CreateClient();

		var first = await client.GetAsync(SearchUrl);
		Assert.Equal(HttpStatusCode.OK, first.StatusCode);
		int calls = igdb.Calls;

		var second = await client.GetAsync(OtherSearchUrl);
		Assert.Equal(HttpStatusCode.OK, second.StatusCode);
		Assert.True(igdb.Calls > calls);
	}

	[Fact]
	public async Task Cache_DoesNotSaveConnectionErrors()
	{
		var igdb = FakeIgdbHandler.Throws(new HttpRequestException("Connection Denied"));
		await using var app = new ApiFactory(igdb);
		var client = app.CreateClient();

		var first = await client.GetAsync("/api/games/3498");
		Assert.Equal(HttpStatusCode.BadGateway, first.StatusCode);
		int calls = igdb.Calls;

		var second = await client.GetAsync("/api/games/3498");
		Assert.Equal(HttpStatusCode.BadGateway, second.StatusCode);
		Assert.True(igdb.Calls > calls);
	}
	
	[Theory]
	[MemberData(nameof(EndpointsPerCode))]
	public async Task Cache_DoesNotSaveIgdbErrors( string url, HttpStatusCode errorCode )
	{
		var igdb = FakeIgdbHandler.Status(errorCode, ErrorBody);
		await using var app = new ApiFactory(igdb);
		var client = app.CreateClient();

		var first = await client.GetAsync(url);
		Assert.Equal(HttpStatusCode.BadGateway, first.StatusCode);
		int calls = igdb.Calls;

		await client.GetAsync(url);
		Assert.True(igdb.Calls > calls);
	}

	[Theory]
	[MemberData(nameof(EndpointsPerCode))]
	public async Task Cache_RecoversWhenIgdbRecovers( string url, HttpStatusCode errorCode )
	{
		bool failing = true;
		var igdb = new FakeIgdbHandler((_,_,_) =>
			Task.FromResult(failing
				? FakeIgdbHandler.Json(errorCode, ErrorBody)
				: FakeIgdbHandler.Json(HttpStatusCode.OK, ValidBody)));
		await using var app = new ApiFactory(igdb);
		var client = app.CreateClient();

		var first = await client.GetAsync(url);
		Assert.Equal(HttpStatusCode.BadGateway, first.StatusCode);

		failing = false;
		var second = await client.GetAsync(url);
		Assert.Equal(HttpStatusCode.OK, second.StatusCode);
		int calls = igdb.Calls;

		var third = await client.GetAsync(url);
		Assert.Equal(HttpStatusCode.OK, third.StatusCode);
		Assert.Equal(calls, igdb.Calls);
	}

	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task Cache_DoesNotSaveBrokenJson( string url )
	{
		var igdb = FakeIgdbHandler.Status(HttpStatusCode.OK, BrokenBody);
		await using var app = new ApiFactory(igdb);
		var client = app.CreateClient();

		var first = await client.GetAsync(url);
		Assert.Equal(HttpStatusCode.BadGateway, first.StatusCode);
		int calls = igdb.Calls;

		var second = await client.GetAsync(url);
		Assert.Equal(HttpStatusCode.BadGateway, second.StatusCode);
		Assert.True(igdb.Calls > calls);
	}
}