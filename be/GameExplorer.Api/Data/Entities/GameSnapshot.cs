namespace GameExplorer.Api.Data.Entities;

// Copy of a game saved by at least one user (favorites, wishlist, tier list).
public class GameSnapshot
{
	public int GameId { get; set; }
	public string Slug { get; set; } = "";
	public string Name { get; set; } = "";
	public string? Image { get; set; }
	public DateOnly? ReleaseDate { get; set; }
	public double Rating { get; set; }
	public int? Metacritic { get; set; }

	// Full GameDetails as JSON (jsonb column)
	public string? Details { get; set; }

	// When the copy was last taken from IGDB. Always UTC (timestamptz)
	public DateTime UpdatedAt { get; set; }
}
