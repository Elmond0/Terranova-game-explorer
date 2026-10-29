using System.Text.Json;
using GameExplorer.Api.Data;
using GameExplorer.Api.Data.Entities;
using GameExplorer.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GameExplorer.Api.Tests.Data;

public class GameCacheStoreTests(PostgresFixture pg) : IClassFixture<PostgresFixture>
{
	static GameDetails NewGame(int id, string slug, double rating = 4.2) => new()
	{
		Id = id,
		Slug = slug,
		Name = $"Game {id}",
		Rating = rating,
		ReleaseDate = new DateOnly(2020, 5, 1),
		About = "x",
	};

	async Task SaveAsync(GameDetails game)
	{
		await using AppDbContext db = pg.CreateContext();
		await new GameCacheStore(db).SaveAsync(game, default);
	}

	[Fact]
	public async Task Saved_game_is_read_back_with_details_and_utc_time()
	{
		await SaveAsync(NewGame(1, "one"));

		await using AppDbContext db = pg.CreateContext();
		GameCacheEntry? entry = await new GameCacheStore(db).FindAsync(1, default);

		Assert.NotNull(entry);
		Assert.Equal("one", entry.Slug);
		GameDetails? details = JsonSerializer.Deserialize<GameDetails>(entry.Details!);
		Assert.Equal("x", details!.About);
		Assert.Equal(DateTimeKind.Utc, entry.UpdatedAt.Kind);
	}

	[Fact]
	public async Task Saving_twice_keeps_one_updated_row()
	{
		await SaveAsync(NewGame(2, "two"));
		GameDetails changed = NewGame(2, "two");
		changed.Name = "Renamed";
		await SaveAsync(changed);

		await using AppDbContext db = pg.CreateContext();
		GameCacheEntry row = await db.GameCache.SingleAsync(g => g.RawgId == 2);
		Assert.Equal("Renamed", row.Name);
	}

	[Fact]
	public async Task Rating_7_is_rejected()
	{
		var ex = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(NewGame(3, "three", rating: 7)));

		Assert.Equal("ck_game_cache_rating", Assert.IsType<PostgresException>(ex.InnerException).ConstraintName);
	}

	[Fact]
	public async Task Same_slug_twice_is_rejected()
	{
		await SaveAsync(NewGame(4, "dup"));

		var ex = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(NewGame(5, "dup")));

		Assert.Equal("ix_game_cache_slug", Assert.IsType<PostgresException>(ex.InnerException).ConstraintName);
	}
}