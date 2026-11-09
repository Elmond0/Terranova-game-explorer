using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.IgdbDtos;

internal class IgdbGame : IIgdbEntity
{
	public static string Url => "games";
	public static string Fields => "fields id, name, slug, rating, first_release_date, cover.*, genres.*, platforms.*;";
	
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
	public long Released { get; set; }

	[JsonPropertyName("platforms")]
	public List<IgdbPlatform>? Platforms { get; set; }

	[JsonPropertyName("genres")]
	public List<IgdbGenre>? Genres { get; set; }
}