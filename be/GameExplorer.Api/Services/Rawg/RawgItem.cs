using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.Rawg;

internal class RawgItem
{
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("slug")]
	public string Slug { get; set; } = "";

	[JsonPropertyName("name")]
	public string Name { get; set; } = "";
}