using System.Net;
using System.Reflection;
using Xunit;

namespace GameExplorer.Api.Tests;

public class HealthTest
{
	private const string url = "/health";

	[Fact]
	public async Task Health_responds200whenHealthy()
	{
		await using var app = new ApiFactory(FakeRawgHandler.Status(HttpStatusCode.OK));
		var res = await app.CreateClient().GetAsync(url);

		Assert.Equal(HttpStatusCode.OK, res.StatusCode);
		Assert.Equal("Healthy", await res.Content.ReadAsStringAsync());
	}

	[Fact]
	public async Task Health_responds200whenRawgIsDown()
	{
		var fakeRawg = FakeRawgHandler.Throws(new HttpRequestException("FakeRawg is down"));
		await using var app = new ApiFactory(fakeRawg);

		var res = await app.CreateClient().GetAsync(url);

		Assert.Equal(HttpStatusCode.OK, res.StatusCode);
		Assert.Equal(0, fakeRawg.Calls);
	}
}