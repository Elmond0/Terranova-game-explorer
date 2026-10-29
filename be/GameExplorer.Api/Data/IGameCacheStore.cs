using GameExplorer.Api.Data.Entities;
using GameExplorer.Shared.Models;

namespace GameExplorer.Api.Data;

/// <summary> Games downloaded from RAWG, saved in the game_cache table. </summary>
public interface IGameCacheStore
{
	/// <summary> The saved game, or null if it was never saved. Check it with GameCachePolicy.IsFresh.</summary>
	Task<GameCacheEntry?> FindAsync(int rawgId, CancellationToken ct);

	/// <summary>Inserts or updates the game; Details is GameDetails as JSON (System.Text.Json defaults).</summary>
	Task SaveAsync(GameDetails game, CancellationToken ct);
}
