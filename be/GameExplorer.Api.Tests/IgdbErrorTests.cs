using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using Xunit;

namespace GameExplorer.Api.Tests;

public class IgdbErrorTests : TestData
{
	private static async Task AssertBadGateway( HttpResponseMessage res )
	{
		Assert.Equal(HttpStatusCode.BadGateway, res.StatusCode);
		Assert.Equal("application/problem+json", res.Content.Headers.ContentType?.MediaType);
		
		var body = await res.Content.ReadAsStringAsync();
		using var json = JsonDocument.Parse(body);
		Assert.Equal(502, json.RootElement.GetProperty("status").GetInt32());

		Assert.DoesNotContain(ApiFactory.FakeClientId, body);
		Assert.DoesNotContain(ApiFactory.FakeClientSecret, body);
		Assert.DoesNotContain(ApiFactory.FakeToken, body);

		Assert.DoesNotContain("api.igdb.com", body);
		Assert.DoesNotContain("id.twitch.tv", body);

		Assert.DoesNotContain("Exception", body);
	}

	[Theory]
	[MemberData(nameof(EndpointsPerCode))]
	public async Task App_responds502whenIgdbRespondsError( string url, HttpStatusCode errorCode )
	{
		await using var app = new ApiFactory(FakeIgdbHandler.Status(errorCode, ErrorBody));
		var res = await app.CreateClient().GetAsync(url);

		await AssertBadGateway(res);
	}

	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task App_responds502whenIgdbDoesNotRespond( string url )
	{
		await using var app = new ApiFactory(FakeIgdbHandler.Throws(new HttpRequestException("Connection denied.")));
		var res = await app.CreateClient().GetAsync(url);

		await AssertBadGateway(res);
	}

	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task App_responds502whenIgdbInTimeout( string url )
	{
		await using var app = new ApiFactory(FakeIgdbHandler.Hangs(), timeout: TimeSpan.FromMilliseconds(300));
		var res = await app.CreateClient().GetAsync(url);

		await AssertBadGateway(res);
	}
	
	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task App_responds502whenBodyIsNotAnArray( string url )
	{
		await using var app = new ApiFactory(FakeIgdbHandler.Status(HttpStatusCode.OK, """{"id":3498,"name":"GTA V"}"""));
		var res = await app.CreateClient().GetAsync(url);
 
		await AssertBadGateway(res);
	}

	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task App_responds502whenTwitchTokenFails( string url )
	{
		var igdb = FakeIgdbHandler.Status(HttpStatusCode.OK, ValidBody);
		var twitch = FakeIgdbHandler.Status(HttpStatusCode.BadRequest, ErrorBody);
		await using var app = new ApiFactory(igdb, twitch: twitch);
 
		var res = await app.CreateClient().GetAsync(url);
 
		await AssertBadGateway(res);
		Assert.Equal(0, igdb.Calls);
	}

	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task App_responds502whenJsonIsBroken( string url )
	{
		await using var app = new ApiFactory(FakeIgdbHandler.Status(HttpStatusCode.OK, BrokenBody));
		var res = await app.CreateClient().GetAsync(url);

		await AssertBadGateway(res);
	}

	[Fact]
	public async Task App_responds404whenGameDoesNotExist()
	{
		await using var app = new ApiFactory(FakeIgdbHandler.Status(HttpStatusCode.OK, EmptyBody));
		var res = await app.CreateClient().GetAsync("/api/games/99999999");

		Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
	}
	
	[Theory]
	[InlineData("/api/games/top-rated")]
	[InlineData("/api/games/recent")]
	[InlineData("/api/games/upcoming")]
	public async Task App_responds200WithEmptyListWhenIgdbHasNoGames( string url )
	{
		await using var app = new ApiFactory(FakeIgdbHandler.Status(HttpStatusCode.OK, EmptyBody));
		var res = await app.CreateClient().GetAsync(url);
 
		Assert.Equal(HttpStatusCode.OK, res.StatusCode);
		using var json = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
		Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
		Assert.Equal(0, json.RootElement.GetArrayLength());
	}

}