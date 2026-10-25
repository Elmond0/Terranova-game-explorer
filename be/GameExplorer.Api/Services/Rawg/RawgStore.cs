using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.Rawg;

internal class RawgStore
{
	[JsonPropertyName("id")]
	public int	Id { get; set; }

	[JsonPropertyName("slug")]
	public string	Slug { get; set; } = "";

	[JsonPropertyName("name")]
	public string	Name { get; set; } = "";

	[JsonPropertyName("domain")]
	public string	Domain { get; set; } = "";
}

