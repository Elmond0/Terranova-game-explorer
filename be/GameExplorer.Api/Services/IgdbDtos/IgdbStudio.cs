using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.IgdbDtos;

internal class IgdbStudio : IIgdbEntity
{
	public static string Url => "companies";
	public static string Fields => "fields id, name, slug, rating, first_release_date, cover.*, genres.*, platforms.*;";
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("slug")]
	public string Slug { get; set; } = "";

	[JsonPropertyName("name")]
	public string Name { get; set; } = "";

	[JsonPropertyName("games_count")]
	public int GamesId { get; set; }
	
	[JsonPropertyName("country")]
	public string Country { get; set; } = "";
}