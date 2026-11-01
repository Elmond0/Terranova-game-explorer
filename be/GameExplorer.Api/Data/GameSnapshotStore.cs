using System.Text.Json;
using GameExplorer.Api.Data.Entities;
using GameExplorer.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GameExplorer.Api.Data;

public class GameSnapshotStore(AppDbContext db)
{
	public async Task SaveAsync(GameDetails game, CancellationToken ct)
	{
		GameSnapshot entry = new()
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

		GameSnapshot? existing = await db.GameSnapshots.FindAsync([entry.RawgId], ct);
		if (existing is null)
			db.GameSnapshots.Add(entry);
		else
			db.Entry(existing).CurrentValues.SetValues(entry);

		try
		{
			await db.SaveChangesAsync(ct);
		}
		catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "pk_game_snapshots" })
		{
			// Another request saved the same game at the same moment: same data, nothing to do
			db.Entry(entry).State = EntityState.Detached;
		}
	}
}
