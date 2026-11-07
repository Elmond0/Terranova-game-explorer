using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using GameExplorer.Api.Data;
using Microsoft.EntityFrameworkCore;
using GameExplorer.Shared.Models;
using Npgsql;

namespace GameExplorer.Api.Tests.Data;

public class MeEndpointsTests(PostgresFixture pg) : IClassFixture<PostgresFixture>
{
	const int MissingGame = 404;
	const int BrokenGame = 500;

	WebApplicationFactory<Program> WithRealDatabase(ApiFactory api) =>
			api.WithWebHostBuilder(b =>
					b.UseSetting("ConnectionStrings:Default", pg.ConnectionString));

	static FakeRawgHandler FakeGames() => new((request, _, _) =>
	{
		int id = int.Parse(request.RequestUri!.Segments[3].TrimEnd('/'));

		HttpResponseMessage response = id switch
		{
			MissingGame => FakeRawgHandler.Json(HttpStatusCode.NotFound, "{}"),
			BrokenGame => FakeRawgHandler.Json(HttpStatusCode.InternalServerError, "{}"),
			_ => FakeRawgHandler.Json(HttpStatusCode.OK, $$"""{"id":{{id}},"name":"Game {{id}}","slug":"game-{{id}}","results":[]}"""),
		};

		return Task.FromResult(response);
	});

	static async Task<HttpClient> LoggedInClient(WebApplicationFactory<Program> app)
	{
		HttpClient client = app.CreateClient();
		var credentials = new { email = $"{Guid.NewGuid()}@test.local", password = "Password1!" };

		HttpResponseMessage register = await client.PostAsJsonAsync("/api/account/register", credentials);
		register.EnsureSuccessStatusCode();

		HttpResponseMessage login = await client.PostAsJsonAsync("/api/account/login?useCookies=true", credentials);
		login.EnsureSuccessStatusCode();

		return client;
	}

	static async Task<List<int>> Ids(HttpClient client, string url)
	{
		List<Game>? games = await client.GetFromJsonAsync<List<Game>>(url);
		return games!.Select(g => g.Id).ToList();
	}

	// A user cannot see or change another user's lists; favorites and wishlist stay separate
	[Fact]
	public async Task Users_see_only_their_own_lists_and_lists_are_separate()
	{
		await using var api = new ApiFactory(FakeGames());
		var app = WithRealDatabase(api);
		HttpClient userA = await LoggedInClient(app);
		HttpClient userB = await LoggedInClient(app);

		await userA.PutAsync("/api/me/favorites/30", null);
		await userB.PutAsync("/api/me/favorites/31", null);
		await userA.PutAsync("/api/me/wishlist/32", null);

		await userB.DeleteAsync("/api/me/favorites/30");

		Assert.Equal([30], await Ids(userA, "/api/me/favorites"));
		Assert.Equal([31], await Ids(userB, "/api/me/favorites"));
		Assert.Equal([32], await Ids(userA, "/api/me/wishlist"));
		Assert.Empty(await Ids(userB, "/api/me/wishlist"));
	}

	// PUT of a game that does not exist returns 404 and writes nothing to the database
	[Fact]
	public async Task Missing_game_returns_404_and_saves_nothing()
	{
		await using var api = new ApiFactory(FakeGames());
		var app = WithRealDatabase(api);
		HttpClient client = await LoggedInClient(app);

		HttpResponseMessage response = await client.PutAsync($"/api/me/favorites/{MissingGame}", null);

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

		await using AppDbContext db = pg.CreateContext();
		Assert.False(await db.GameSnapshots.AnyAsync(g => g.GameId == MissingGame));
		Assert.False(await db.Favorites.AnyAsync(f => f.GameId == MissingGame));
	}

	// With the external API down, PUT returns 502 and GET still works without calling it
	[Fact]
	public async Task Api_down_put_returns_502_and_get_still_works()
	{
		FakeRawgHandler rawg = FakeGames();
		await using var api = new ApiFactory(rawg);
		var app = WithRealDatabase(api);
		HttpClient client = await LoggedInClient(app);

		await client.PutAsync("/api/me/favorites/40", null);

		HttpResponseMessage put = await client.PutAsync($"/api/me/favorites/{BrokenGame}", null);
		Assert.Equal(HttpStatusCode.BadGateway, put.StatusCode);

		int callsBefore = rawg.Calls;
		Assert.Equal([40], await Ids(client, "/api/me/favorites"));
		Assert.Equal(callsBefore, rawg.Calls);
	}
	
	// Eight identical PUTs at the same time all return 204 and leave a single row
	[Fact]
	public async Task Eight_equal_puts_leave_one_row()
	{
		await using var api = new ApiFactory(FakeGames());
		var app = WithRealDatabase(api);
		HttpClient client = await LoggedInClient(app);

		Task<HttpResponseMessage>[] puts = Enumerable.Range(0, 8)
				.Select(_ => client.PutAsync("/api/me/favorites/10", null))
				.ToArray();
		HttpResponseMessage[] responses = await Task.WhenAll(puts);

		Assert.All(responses, r => Assert.Equal(HttpStatusCode.NoContent, r.StatusCode));

		await using AppDbContext db = pg.CreateContext();
		Assert.Equal(1, await db.Favorites.CountAsync(f => f.GameId == 10));
	}

	// DELETE of a game that is not in the list returns 204
	[Fact]
	public async Task Delete_of_missing_game_returns_204()
	{
		await using var api = new ApiFactory(FakeGames());
		var app = WithRealDatabase(api);
		HttpClient client = await LoggedInClient(app);

		HttpResponseMessage response = await client.DeleteAsync("/api/me/favorites/1");
		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
	}

	// All six endpoints return 401 without login
	[Theory]
	[InlineData("GET", "/api/me/favorites")]
	[InlineData("PUT", "/api/me/favorites/1")]
	[InlineData("DELETE", "/api/me/favorites/1")]
	[InlineData("GET", "/api/me/wishlist")]
	[InlineData("PUT", "/api/me/wishlist/1")]
	[InlineData("DELETE", "/api/me/wishlist/1")]
	public async Task Without_login_returns_401(string method, string url)
	{
		await using var api = new ApiFactory(FakeGames());
		var app = WithRealDatabase(api);
		HttpClient client = app.CreateClient();

		var request = new HttpRequestMessage(new HttpMethod(method), url);
		HttpResponseMessage response = await client.SendAsync(request);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	// GET returns the newest game first, and a repeated PUT does not move it
	[Fact]
	public async Task Get_returns_newest_first_and_repeated_put_does_not_move_it()
	{
		await using var api = new ApiFactory(FakeGames());
		var app = WithRealDatabase(api);
		HttpClient client = await LoggedInClient(app);

		await client.PutAsync("/api/me/favorites/20", null);
		await client.PutAsync("/api/me/favorites/21", null);
		await client.PutAsync("/api/me/favorites/20", null);

		List<Game>? favorites = await client.GetFromJsonAsync<List<Game>>("/api/me/favorites");

		Assert.Equal([21, 20], favorites!.Select(g => g.Id));
	}
	
	// Deleting a user removes their rows; deleting a game that is still in a list is refused (23001)
	[Fact]
	public async Task Deleting_user_removes_rows_and_used_game_cannot_be_deleted()
	{
		await using var api = new ApiFactory(FakeGames());
		var app = WithRealDatabase(api);
		HttpClient client = await LoggedInClient(app);

		await client.PutAsync("/api/me/favorites/50", null);
		await client.PutAsync("/api/me/wishlist/51", null);

		await using AppDbContext db = pg.CreateContext();
		string userId = await db.Favorites.Where(f => f.GameId == 50).Select(f => f.UserId).SingleAsync();

		PostgresException error = await Assert.ThrowsAsync<PostgresException>(() =>
				db.GameSnapshots.Where(g => g.GameId == 50).ExecuteDeleteAsync());
		Assert.Equal(PostgresErrorCodes.RestrictViolation, error.SqlState);

		await db.Users.Where(u => u.Id == userId).ExecuteDeleteAsync();

		Assert.False(await db.Favorites.AnyAsync(f => f.UserId == userId));
		Assert.False(await db.WishlistItems.AnyAsync(w => w.UserId == userId));
	}
}
