namespace GameExplorer.Shared.Models;

public class Studio
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public string Slug { get; set; } = "";
	public string? Image { get; set; }
	public string? About { get; set; }
	public int GamesNbr { get; set; }
}