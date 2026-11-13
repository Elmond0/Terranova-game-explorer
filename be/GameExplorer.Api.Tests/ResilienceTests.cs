using System.Net;
using Xunit;
 
namespace GameExplorer.Api.Tests;
 
public class ResilienceTest : TestData
{
	private const string Url = "/api/genres";
 
	[Theory]
	[InlineData(HttpStatusCode.InternalServerError)]
	[InlineData(HttpStatusCode.ServiceUnavailable)]
	[InlineData(HttpStatusCode.TooManyRequests)]
	public async Task Retry_RecoversFromTransientError( HttpStatusCode errorCode )
	{
		var igdb = new FakeIgdbHandler((_,n,_) =>
			Task.FromResult(n == 1
				? FakeIgdbHandler.Json(errorCode, ErrorBody)
				: FakeIgdbHandler.Json(HttpStatusCode.OK, ValidBody)));
		await using var app = new ApiFactory(igdb);
 
		var client = await app.CreateClient().GetAsync(Url);
 
		Assert.Equal(HttpStatusCode.OK, client.StatusCode);
		Assert.Equal(2, igdb.Calls);
	}
 
	[Fact]
	public async Task Retry_GivesUpAfterMaxAttempts()
	{
		var igdb = FakeIgdbHandler.Status(HttpStatusCode.ServiceUnavailable, ErrorBody);
		await using var app = new ApiFactory(igdb);
 
		var client = await app.CreateClient().GetAsync(Url);
 
		Assert.Equal(HttpStatusCode.BadGateway, client.StatusCode);
		Assert.Equal(3, igdb.Calls);	// 1 tentativo + MaxRetryAttempts (2)
	}
 
	// 401 escluso: a differenza degli altri errori client l'IgdbAuthHandler lo gestisce (vedi test sotto)
	[Theory]
	[InlineData(HttpStatusCode.BadRequest)]
	[InlineData(HttpStatusCode.Forbidden)]
	[InlineData(HttpStatusCode.NotFound)]
	public async Task Retry_DoesNotRetryClientError( HttpStatusCode errorCode )
	{
		var igdb = FakeIgdbHandler.Status(errorCode, ErrorBody);
		await using var app = new ApiFactory(igdb);
 
		var client = await app.CreateClient().GetAsync(Url);
 
		Assert.Equal(HttpStatusCode.BadGateway, client.StatusCode);
		Assert.Equal(1, igdb.Calls);
	}
 
	[Fact]
	public async Task Retry_RecoversFromConnectionErrors()
	{
		var igdb = new FakeIgdbHandler((_,n,_) =>
			n == 1
			? Task.FromException<HttpResponseMessage>(new HttpRequestException("Connection denied"))
			: Task.FromResult(FakeIgdbHandler.Json(HttpStatusCode.OK, ValidBody)));
		await using var app = new ApiFactory(igdb);
 
		var client = await app.CreateClient().GetAsync(Url);
 
		Assert.Equal(HttpStatusCode.OK, client.StatusCode);
		Assert.Equal(2, igdb.Calls);
	}
 
	[Fact]
	public async Task AttemptTimeout_TriggersRetry()
	{
		var igdb = new FakeIgdbHandler(async (_, n, ct) =>
		{
			if (n == 1)
				await Task.Delay(Timeout.InfiniteTimeSpan, ct);
			return FakeIgdbHandler.Json(HttpStatusCode.OK, ValidBody);
		});
		await using var app = new ApiFactory(igdb);
 
		var client = await app.CreateClient().GetAsync(Url);
 
		Assert.Equal(HttpStatusCode.OK, client.StatusCode);
		Assert.Equal(2, igdb.Calls);
	}
 
	// --- Token Twitch (IgdbAuthHandler) ---
 
	[Fact]
	public async Task Unauthorized_RefreshesTokenAndRetriesOnce()
	{
		var igdb = new FakeIgdbHandler((_, n, _) =>
			Task.FromResult(n == 1
				? FakeIgdbHandler.Json(HttpStatusCode.Unauthorized, ErrorBody)
				: FakeIgdbHandler.Json(HttpStatusCode.OK, ValidBody)));
		await using var app = new ApiFactory(igdb);
 
		var client = await app.CreateClient().GetAsync(Url);
 
		Assert.Equal(HttpStatusCode.OK, client.StatusCode);
		Assert.Equal(2, igdb.Calls);
		Assert.Equal(2, app.Twitch.Calls);	// token iniziale + token rinnovato
	}
 
	[Fact]
	public async Task Unauthorized_GivesUpAfterOneRefresh()
	{
		var igdb = FakeIgdbHandler.Status(HttpStatusCode.Unauthorized, ErrorBody);
		await using var app = new ApiFactory(igdb);
 
		var client = await app.CreateClient().GetAsync(Url);
 
		Assert.Equal(HttpStatusCode.BadGateway, client.StatusCode);
		Assert.Equal(2, igdb.Calls);
		Assert.Equal(2, app.Twitch.Calls);
	}
 
	[Fact]
	public async Task Token_IsRequestedOnceAndReusedAcrossRequests()
	{
		var igdb = FakeIgdbHandler.Status(HttpStatusCode.OK, ValidBody);
		await using var app = new ApiFactory(igdb);
		var client = app.CreateClient();
 
		// Endpoint diversi: nessuna delle risposte e' in cache
		Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/games/top-rated")).StatusCode);
		Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/games/recent")).StatusCode);
		Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/games/upcoming")).StatusCode);
 
		Assert.Equal(3, igdb.Calls);
		Assert.Equal(1, app.Twitch.Calls);
	}
}
