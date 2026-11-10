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

		Assert.DoesNotContain(ApiFactory.FakeKey, body);
		Assert.DoesNotContain("api.rawg.io", body);
		Assert.DoesNotContain("Exception", body);
	}

	[Theory]
	[MemberData(nameof(EndpointsPerCode))]
	public async Task App_responds502whenRawgRespondsError( string url, HttpStatusCode errorCode )
	{
		await using var app = new ApiFactory(FakeIgbdHandler.Status(errorCode, """{"error":"error"}"""));
		var res = await app.CreateClient().GetAsync(url);

		await AssertBadGateway(res);
	}

	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task App_responds502whenRawgDoesNotRespond( string url )
	{
		await using var app = new ApiFactory(FakeIgbdHandler.Throws(new HttpRequestException("Connection denied.")));
		var res = await app.CreateClient().GetAsync(url);

		await AssertBadGateway(res);
	}

	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task App_responds502whenRawgInTimeout( string url )
	{
		await using var app = new ApiFactory(FakeIgbdHandler.Hangs(), timeout: TimeSpan.FromMilliseconds(300));
		var res = await app.CreateClient().GetAsync(url);

		await AssertBadGateway(res);
	}

	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task App_responds502whenJsonIsBroken( string url )
	{
		await using var app = new ApiFactory(FakeIgbdHandler.Status(HttpStatusCode.OK, "This is not Jason D:"));
		var res = await app.CreateClient().GetAsync(url);

		await AssertBadGateway(res);
	}

	[Fact]
	public async Task App_responds404whenGameDoesNotExist()
	{
		await using var app = new ApiFactory(FakeIgbdHandler.Status(HttpStatusCode.NotFound));
		var res = await app.CreateClient().GetAsync("/api/games/99999999");

		Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
	}
}