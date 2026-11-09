using System.Text.Json;
using GameExplorer.Api.Data.Entities;
using GameExplorer.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GameExplorer.Api.Data;

public class GameSnapshotStore(AppDbContext db)
{
	private static readonly string[] ValidOrders = ["added", "name", "released", "rating"];

	public static bool IsValidOrdering(string? ordering) =>
		string.IsNullOrWhiteSpace(ordering) || ValidOrders.Contains(ordering.TrimStart('-'));

	public async Task SaveAsync(Game game, CancellationToken ct)
	{
		List<Genre> genres = game.Genres.DistinctBy(g => g.Id).ToList();
		List<Platform> platforms = game.Platforms.DistinctBy(p => p.Id).ToList();

		string genresJson = JsonSerializer.Serialize(genres);
		string platformsJson = JsonSerializer.Serialize(platforms);

		await db.Database.ExecuteSqlAsync($"""
			INSERT INTO game_snapshots (game_id, slug, name, image, release_date, rating, updated_at, genres, platforms)
			VALUES ({game.Id}, {game.Slug}, {game.Name}, {game.Image?.Url}, {game.ReleaseDate}, {game.Rating}, {DateTime.UtcNow}, {genresJson}::jsonb, {platformsJson}::jsonb)
			ON CONFLICT (game_id) DO UPDATE SET
				slug = excluded.slug, name = excluded.name, image = excluded.image,
				release_date = excluded.release_date, rating = excluded.rating, updated_at = excluded.updated_at,
				genres = excluded.genres, platforms = excluded.platforms
			""", ct);
	}

	public async Task<List<Game>> GetListAsync<T>(IQueryable<T> entries, int[] platformIds, string? ordering, CancellationToken ct)
		where T : class, IListEntry
	{
		foreach (int id in platformIds)
			entries = entries.Where(e => e.Game.Platforms.Any(p => p.Id == id));

		if (string.IsNullOrWhiteSpace(ordering))
			ordering = "-added";

		bool desc = ordering.StartsWith('-');
		IOrderedQueryable<T> ordered = ordering.TrimStart('-') switch
		{
			"name" => desc ? entries.OrderByDescending(e => e.Game.Name) : entries.OrderBy(e => e.Game.Name),
			"rating" => desc ? entries.OrderByDescending(e => e.Game.Rating) : entries.OrderBy(e => e.Game.Rating),
			"released" => desc
				? entries.OrderBy(e => e.Game.ReleaseDate == null).ThenByDescending(e => e.Game.ReleaseDate)
				: entries.OrderBy(e => e.Game.ReleaseDate == null).ThenBy(e => e.Game.ReleaseDate),
			_ => desc ? entries.OrderByDescending(e => e.AddedAt) : entries.OrderBy(e => e.AddedAt),
		};
		entries = desc ? ordered.ThenByDescending(e => e.Game.GameId) : ordered.ThenBy(e => e.Game.GameId);

		return await entries
			.AsNoTracking()
			.Select(e => e.Game)
			.Select(GameSnapshotMapper.ToGame)
			.ToListAsync(ct);
	}
}
