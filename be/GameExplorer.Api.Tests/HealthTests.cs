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
		await using var app = new ApiFactory(FakeIgdbHandler.Status(HttpStatusCode.OK));
		var res = await app.CreateClient().GetAsync(url);

		Assert.Equal(HttpStatusCode.OK, res.StatusCode);
		Assert.Equal("Healthy", await res.Content.ReadAsStringAsync());
	}

	[Fact]
	public async Task Health_responds200whenRawgIsDown()
	{
		var fakeIgdb = FakeIgdbHandler.Throws(new HttpRequestException("FakeIgdb is down"));
		await using var app = new ApiFactory(fakeIgdb);

		var res = await app.CreateClient().GetAsync(url);

		Assert.Equal(HttpStatusCode.OK, res.StatusCode);
		Assert.Equal(0, fakeIgdb.Calls);
		Assert.Equal(0, app.Twitch.Calls);
	}
}