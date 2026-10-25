namespace GameExplorer.Shared.Models;

public class Store
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public string Slug { get; set; } = "";
	public string? Domain { get; set; }
}