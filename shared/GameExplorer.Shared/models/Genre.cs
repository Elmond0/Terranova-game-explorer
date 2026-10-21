namespace GameExplorer.Shared.Models;

public class Genre
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public string Slug { get; set; } = "";
	public string? Description { get; set; }
}