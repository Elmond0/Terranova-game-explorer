using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.IgdbDtos;

internal class IgdbGame
{
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("slug")]
	public string Slug { get; set; } = "";
	
	[JsonPropertyName("name")]
	public string Name { get; set; } = "";

	[JsonPropertyName("rating")]
	public double Rating { get; set; }

	[JsonPropertyName("cover")]
	public IgdbImage? Cover { get; set; }

	[JsonPropertyName("first_release_date")]
	public DateOnly? Released { get; set; }

	[JsonPropertyName("platforms")]
	public List<IgdbItem>? Platforms { get; set; }

	[JsonPropertyName("genres")]
	public List<IgdbItem>? Genres { get; set; }
}