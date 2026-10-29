using System.Text.Json;
using GameExplorer.Api.Data.Entities;
using GameExplorer.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GameExplorer.Api.Data;

public class GameCacheStore(AppDbContext db) : IGameCacheStore
{
	public Task<GameCacheEntry?> FindAsync(int rawgId, CancellationToken ct)
	{
		// Read only: no change tracking needed
		return db.GameCache.AsNoTracking().FirstOrDefaultAsync(g => g.RawgId == rawgId, ct);
	}

	public async Task SaveAsync(GameDetails game, CancellationToken ct)
	{
		GameCacheEntry entry = new()
		{
			RawgId = game.Id,
			Slug = game.Slug,
			Name = game.Name,
			Image = game.Image,
			ReleaseDate = game.ReleaseDate,
			Rating = game.Rating,
			Metacritic = game.Metacritic,
			Details = JsonSerializer.Serialize(game),
			UpdatedAt = DateTime.UtcNow,
		};

		GameCacheEntry? existing = await db.GameCache.FindAsync([entry.RawgId], ct);
		if (existing is null)
			db.GameCache.Add(entry);
		else
			db.Entry(existing).CurrentValues.SetValues(entry);

		try
		{
			await db.SaveChangesAsync(ct);
		}
		catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "pk_game_cache" })
		{
			// Another request saved the same game at the same moment: same data, nothing to do
		}
	}
}
