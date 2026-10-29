using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using Xunit;

namespace GameExplorer.Api.Tests;

public class RawgErrorTests
{
	private static readonly string[] Endpoints =
	[
		"api/games/3498",
	];

	private static readonly HttpStatusCode[] ErrorCodes =
	[
		HttpStatusCode.BadRequest,					// 400
		HttpStatusCode.Unauthorized,				// 401
		HttpStatusCode.Forbidden,					// 403
		HttpStatusCode.TooManyRequests,				// 429
		HttpStatusCode.InternalServerError,			// 500
		HttpStatusCode.BadGateway,					// 502
		HttpStatusCode.ServiceUnavailable,			// 503
		HttpStatusCode.GatewayTimeout,				// 504	
	];

	public static IEnumerable<object[]> EndpointsPerCode()
		=>	from e in Endpoints
			from c in ErrorCodes
			select new object[] { e, c };

	public static IEnumerable<object[]> OnlyEndpoints()
		=> Endpoints.Select(e => new object[] { e });

	[Theory]
	[MemberData(nameof(EndpointsPerCode))]
	public async Task App_responds502whenRawgRespondsError( string url, HttpStatusCode errorCode )
	{
		await using var app = new ApiFactory(FakeRawgHandler.Status(errorCode, """{"error":"error"}"""));
		var res = await app.CreateClient().GetAsync(url);

		await AssertBadGateway(res);
	}

	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task App_responds502whenRawgDoesNotRespond( string url )
	{
		await using var app = new ApiFactory(FakeRawgHandler.Throws(new HttpRequestException("Connection denied.")));
		var res = await app.CreateClient().GetAsync(url);

		await AssertBadGateway(res);
	}

	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task App_responds502whenRawgInTimeout( string url )
	{
		await using var app = new ApiFactory(FakeRawgHandler.Hangs(), timeout: TimeSpan.FromMilliseconds(300));
		var res = await app.CreateClient().GetAsync(url);

		await AssertBadGateway(res);
	}

	[Theory]
	[MemberData(nameof(OnlyEndpoints))]
	public async Task App_responds502whenJsonIsBroken( string url )
	{
		await using var app = new ApiFactory(FakeRawgHandler.Status(HttpStatusCode.OK, "This is not Jason D:"));
		var res = await app.CreateClient().GetAsync(url);

		await AssertBadGateway(res);
	}

	[Fact]
	public async Task App_responds404whenGameDoesNotExist()
	{
		await using var app = new ApiFactory(FakeRawgHandler.Status(HttpStatusCode.NotFound));
		var res = await app.CreateClient().GetAsync("/api/games/99999999");

		Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
	}

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

}