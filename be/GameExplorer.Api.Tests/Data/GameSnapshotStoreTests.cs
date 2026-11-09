using GameExplorer.Api.Data;
using GameExplorer.Api.Data.Entities;
using GameExplorer.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GameExplorer.Api.Tests.Data;

public class GameSnapshotStoreTests(PostgresFixture pg) : IClassFixture<PostgresFixture>
{
	static Game NewGame(int id, double rating = 80, int[]? genres = null, int[]? platforms = null, DateOnly? release = null) => new()
	{
		Id = id,
		Slug = $"game-{id}",
		Name = $"Game {id}",
		Rating = rating,
		ReleaseDate = release,
		Genres = (genres ?? [5]).Select(g => new Genre { Id = g, Name = $"Genre {g}", Slug = $"genre-{g}" }).ToList(),
		Platforms = (platforms ?? [6]).Select(p => new Platform { Id = p, Name = $"Platform {p}", Slug = $"platform-{p}" }).ToList(),
	};

	async Task SaveAsync(Game game)
	{
		await using AppDbContext db = pg.CreateContext();
		await new GameSnapshotStore(db).SaveAsync(game, default);
	}

	async Task<GameSnapshot> LoadAsync(int id)
	{
		await using AppDbContext db = pg.CreateContext();
		return await db.GameSnapshots.AsNoTracking().SingleAsync(g => g.GameId == id);
	}

	async Task<string> NewUserWithFavoritesAsync(params Game[] games)
	{
		string userId = Guid.NewGuid().ToString();
		foreach (Game g in games)
			await SaveAsync(g);

		await using AppDbContext db = pg.CreateContext();
		db.Users.Add(new IdentityUser { Id = userId, UserName = userId });
		DateTime t = DateTime.UtcNow;
		foreach (Game g in games)
			db.Favorites.Add(new Favorite { UserId = userId, GameId = g.Id, AddedAt = t = t.AddSeconds(1) });
		await db.SaveChangesAsync();
		return userId;
	}

	async Task<List<Game>> FavoritesAsync(string userId, int[]? platformIds = null, string? ordering = null)
	{
		await using AppDbContext db = pg.CreateContext();
		return await new GameSnapshotStore(db).GetListAsync(db.Favorites.Where(f => f.UserId == userId), platformIds ?? [], ordering, default);
	}

	// Saves a game and reads back slug, genres, platforms and a UTC timestamp
	[Fact]
	public async Task Saved_game_is_read_back_with_genres_platforms_and_utc_time()
	{
		await SaveAsync(NewGame(1, genres: [5, 12], platforms: [6, 48]));

		GameSnapshot row = await LoadAsync(1);
		Assert.Equal("game-1", row.Slug);
		Assert.Equal([5, 12], row.Genres.Select(g => g.Id).Order());
		Assert.Equal([6, 48], row.Platforms.Select(p => p.Id).Order());
		Assert.Equal(DateTimeKind.Utc, row.UpdatedAt.Kind);
	}

	// Saving the same game twice updates the name and replaces the old genres
	[Fact]
	public async Task Saving_again_updates_the_row_and_replaces_the_genres()
	{
		await SaveAsync(NewGame(2, genres: [5, 12]));
		Game changed = NewGame(2, genres: [31]);
		changed.Name = "Renamed";
		await SaveAsync(changed);

		GameSnapshot row = await LoadAsync(2);
		Assert.Equal("Renamed", row.Name);
		Assert.Equal([31], row.Genres.Select(g => g.Id));
	}

	// A game with no cover and rating 0 is saved without errors
	[Fact]
	public async Task Game_without_cover_and_with_rating_0_is_saved()
	{
		await SaveAsync(NewGame(3, rating: 0));

		GameSnapshot row = await LoadAsync(3);
		Assert.Null(row.Image);
		Assert.Equal(0, row.Rating);
	}

	// Only the cover URL ends up in the image column
	[Fact]
	public async Task Cover_url_is_saved()
	{
		Game game = NewGame(5);
		game.Image = new Image { Id = 7, Url = "//images.igdb.com/cover.jpg", Width = 264, Height = 352 };
		await SaveAsync(game);

		GameSnapshot row = await LoadAsync(5);
		Assert.Equal("//images.igdb.com/cover.jpg", row.Image);
	}

	// The database check constraint rejects a rating above 100
	[Fact]
	public async Task Rating_101_is_rejected()
	{
		var ex = await Assert.ThrowsAsync<PostgresException>(() => SaveAsync(NewGame(4, rating: 101)));

		Assert.Equal("ck_game_snapshots_rating", ex.ConstraintName);
	}

	// Many parallel saves of the same games all succeed and leave each game's genres without duplicates
	[Fact]
	public async Task Parallel_saves_of_the_same_games_do_not_fail()
	{
		Game a = NewGame(10, genres: [100, 101, 102]);
		Game b = NewGame(11, genres: [102, 101, 100]);

		for (int round = 0; round < 10; round++)
			await Task.WhenAll(Enumerable.Range(0, 8).Select(i => SaveAsync(i % 2 == 0 ? a : b)));

		Assert.Equal(3, (await LoadAsync(10)).Genres.Count);
		Assert.Equal(3, (await LoadAsync(11)).Genres.Count);
	}

	// The default favorites list is newest first and includes cover, genres and platforms
	[Fact]
	public async Task Favorites_are_newest_first_with_cover_genres_and_platforms()
	{
		Game withCover = NewGame(21, platforms: [6, 48]);
		withCover.Image = new Image { Id = 8, Url = "//images.igdb.com/21.jpg", Width = 264, Height = 352 };
		string user = await NewUserWithFavoritesAsync(NewGame(20), withCover);

		List<Game> games = await FavoritesAsync(user);

		Assert.Equal([21, 20], games.Select(g => g.Id));
		Assert.Equal(2, games[0].Platforms.Count);
		Assert.Equal("genre-5", games[0].Genres.Single().Slug);
		Assert.Equal("//images.igdb.com/21.jpg", games[0].Image?.Url);
		Assert.Null(games[1].Image);
	}

	// Platform filter (all platforms required) and every ordering, ascending and descending
	[Fact]
	public async Task Favorites_filter_by_platform_and_order_like_the_search()
	{
		string user = await NewUserWithFavoritesAsync(
			NewGame(30, rating: 70, platforms: [6], release: new DateOnly(2020, 1, 1)),
			NewGame(31, rating: 0, platforms: [48]),
			NewGame(32, rating: 90, platforms: [6, 48], release: new DateOnly(2024, 1, 1)));

		Assert.Equal([32, 30], (await FavoritesAsync(user, platformIds: [6], ordering: "-rating")).Select(g => g.Id));
		Assert.Equal([32], (await FavoritesAsync(user, platformIds: [6, 48])).Select(g => g.Id));
		Assert.Equal([32, 30, 31], (await FavoritesAsync(user, ordering: "-rating")).Select(g => g.Id));
		Assert.Equal([32, 30, 31], (await FavoritesAsync(user, ordering: "-released")).Select(g => g.Id));
		Assert.Equal([30, 32, 31], (await FavoritesAsync(user, ordering: "released")).Select(g => g.Id));
		Assert.Equal([30, 31, 32], (await FavoritesAsync(user, ordering: "name")).Select(g => g.Id));
		Assert.Equal([30, 31, 32], (await FavoritesAsync(user, ordering: "added")).Select(g => g.Id));
	}

	// Empty or known orderings are valid, unknown ones are not
	[Fact]
	public void Ordering_follows_the_search_rule()
	{
		Assert.True(GameSnapshotStore.IsValidOrdering(null));
		Assert.True(GameSnapshotStore.IsValidOrdering("-rating"));
		Assert.False(GameSnapshotStore.IsValidOrdering("xyz"));
		Assert.False(GameSnapshotStore.IsValidOrdering("-created"));
	}
}
