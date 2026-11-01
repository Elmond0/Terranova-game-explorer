using System.Text.Json;
using GameExplorer.Api.Data;
using GameExplorer.Api.Data.Entities;
using GameExplorer.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GameExplorer.Api.Tests.Data;

public class GameSnapshotStoreTests(PostgresFixture pg) : IClassFixture<PostgresFixture>
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
		await new GameSnapshotStore(db).SaveAsync(game, default);
	}

	[Fact]
	public async Task Saved_game_is_read_back_with_details_and_utc_time()
	{
		await SaveAsync(NewGame(1, "one"));

		await using AppDbContext db = pg.CreateContext();
		GameSnapshot? entry = await db.GameSnapshots.AsNoTracking().SingleOrDefaultAsync(g => g.RawgId == 1);

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
		GameSnapshot row = await db.GameSnapshots.SingleAsync(g => g.RawgId == 2);
		Assert.Equal("Renamed", row.Name);
	}

	[Fact]
	public async Task Rating_7_is_rejected()
	{
		var ex = await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsync(NewGame(3, "three", rating: 7)));

		Assert.Equal("ck_game_snapshots_rating", Assert.IsType<PostgresException>(ex.InnerException).ConstraintName);
	}

	[Fact]
	public async Task Same_slug_on_two_games_is_accepted()
	{
		await SaveAsync(NewGame(4, "dup"));
		await SaveAsync(NewGame(5, "dup"));

		await using AppDbContext db = pg.CreateContext();
		Assert.Equal(2, await db.GameSnapshots.CountAsync(g => g.Slug == "dup"));
	}
}
