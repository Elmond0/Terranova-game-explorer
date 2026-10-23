using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.Rawg;

internal class RawgStudio
{
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("slug")]
	public string Slug { get; set; } = "";

	[JsonPropertyName("name")]
	public string Name { get; set; } = "";

	[JsonPropertyName("image_background")]
	public string? BackgroundImage { get; set; }

	[JsonPropertyName("games_count")]
	public int GamesCount { get; set; }
}