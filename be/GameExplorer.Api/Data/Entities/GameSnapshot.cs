using GameExplorer.Shared.Models;

namespace GameExplorer.Api.Data.Entities;

public class GameSnapshot
{
	public int GameId { get; set; }
	public string Slug { get; set; } = "";
	public string Name { get; set; } = "";
	public string? Image { get; set; }
	public DateOnly? ReleaseDate { get; set; }
	public double Rating { get; set; }
	public DateTime UpdatedAt { get; set; }
	public List<Genre> Genres { get; set; } = [];
	public List<Platform> Platforms { get; set; } = [];
}
