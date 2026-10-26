using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.Rawg;

internal class RawgGame
{
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("slug")]
	public string Slug { get; set; } = "";
	
	[JsonPropertyName("name")]
	public string Name { get; set; } = "";

	[JsonPropertyName("rating")]
	public double Rating { get; set; }

	[JsonPropertyName("background_image")]
	public string? BackgroundImage { get; set; }

	[JsonPropertyName("released")]
	public DateOnly? Released { get; set; }

	[JsonPropertyName("metacritic")]
	public int? Metacritic { get; set; }

	[JsonPropertyName("platforms")]
	public List<RawgPlatformWrapper>? Platforms { get; set; }

	[JsonPropertyName("genres")]
	public List<RawgItem>? Genres { get; set; }
}