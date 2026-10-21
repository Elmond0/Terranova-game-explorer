using System.Runtime.Versioning;

namespace GameExplorer.Shared.Models;

public class Game
{
	public int Id { get; set; }
	public string Slug { get; set; } = "";
	public string Name { get; set; } = "";
	public double Rating { get; set; }
	public string? Image { get; set; }
	public DateOnly? ReleaseDate { get; set; }
	public int? Metacritic { get; set; }
	public List<Platform> Platforms { get; set; } = [];
	public List<Genre> Genres { get; set; } = [];
}