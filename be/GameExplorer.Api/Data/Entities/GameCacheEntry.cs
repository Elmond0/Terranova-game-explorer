namespace GameExplorer.Api.Data.Entities;

/// <summary> A game downloaded from RAWG, kept to avoid asking RAWG again.</summary>
public class GameCacheEntry
{
	public int RawgId { get; set; }
	public string Slug { get; set; } = "";
	public string Name { get; set; } = "";
	public string? Image { get; set; }
	public DateOnly? ReleaseDate { get; set; }
	public double Rating { get; set; }
	public int? Metacritic { get; set; }

	// Full GameDetails as JSON (jsonb column); null if only seen in a list so far
	public string? Details { get; set; }

	// When it was downloaded from RAWG. Always UTC (timestamptz)
	public DateTime UpdatedAt { get; set; }
}
