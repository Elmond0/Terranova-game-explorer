using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.Rawg;

internal class RawgStoreWrapper
{
	[JsonPropertyName("store")]
	public RawgItem? Store { get; set; }

	[JsonPropertyName("url")]
	public string? Url { get; set; }
}