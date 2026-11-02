using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using System.Net;
using System.Runtime.ExceptionServices;
using Xunit.Abstractions;

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
		var rawg = new FakeRawgHandler((_,n,_) =>
			Task.FromResult(n == 1
				? FakeRawgHandler.Json(errorCode, ErrorBody)
				: FakeRawgHandler.Json(HttpStatusCode.OK, ValidBody)));
		await using var app = new ApiFactory(rawg);

		var client = await app.CreateClient().GetAsync(Url);

		Assert.Equal(HttpStatusCode.OK, client.StatusCode);
		Assert.Equal(2, rawg.Calls);
	}

	[Fact]
	public async Task Retry_GivesUpAfterMaxAttempts()
	{
		var rawg = FakeRawgHandler.Status(HttpStatusCode.ServiceUnavailable, ErrorBody);
		await using var app = new ApiFactory(rawg);

		var client = await app.CreateClient().GetAsync(Url);

		Assert.Equal(HttpStatusCode.BadGateway, client.StatusCode);
		Assert.Equal(3, rawg.Calls);
	}

	[Theory]
	[InlineData(HttpStatusCode.BadRequest)]
	[InlineData(HttpStatusCode.Unauthorized)]
	[InlineData(HttpStatusCode.Forbidden)]
	public async Task Retry_DoesNotRetryClientError( HttpStatusCode errorCode )
	{
		var rawg = FakeRawgHandler.Status(errorCode, ErrorBody);
		await using var app = new ApiFactory(rawg);

		var client = await app.CreateClient().GetAsync(Url);

		Assert.Equal(HttpStatusCode.BadGateway, client.StatusCode);
		Assert.Equal(1, rawg.Calls);
	}

	[Fact]
	public async Task Retry_RecoversFromConnectionErrors()
	{
		var rawg = new FakeRawgHandler((_,n,_) =>
			n == 1
			? Task.FromException<HttpResponseMessage>(new HttpRequestException("Connection denied"))
			: Task.FromResult(FakeRawgHandler.Json(HttpStatusCode.OK, ValidBody)));
		await using var app = new ApiFactory(rawg);

		var client = await app.CreateClient().GetAsync(Url);

		Assert.Equal(HttpStatusCode.OK, client.StatusCode);
		Assert.Equal(2, rawg.Calls);
	}

	[Fact]
	public async Task AttemptTimeout_TriggersRetry()
	{
		var rawg = new FakeRawgHandler(async (_, n, ct) =>
		{
			if (n == 1)
				await Task.Delay(Timeout.InfiniteTimeSpan, ct);
			return FakeRawgHandler.Json(HttpStatusCode.OK, ValidBody);
		});
		await using var app = new ApiFactory(rawg);
		var client = await app.CreateClient().GetAsync(Url);
		Assert.Equal(HttpStatusCode.OK, client.StatusCode);
		Assert.Equal(2, rawg.Calls);
	}
}