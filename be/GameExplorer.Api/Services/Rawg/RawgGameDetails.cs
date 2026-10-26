using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.Rawg;

internal class RawgGameDetails : RawgGame
{
	[JsonPropertyName("description")]
	public string? About { get; set; }

	[JsonPropertyName("website")]
	public string? Website { get; set; }

	[JsonPropertyName("developers")]
	public List<RawgStudio> Developers { get; set; } = [];

	[JsonPropertyName("publishers")]
	public List<RawgStudio> Publishers { get; set; } = [];

	[JsonPropertyName("stores")]
	public List<RawgStoreWrapper> Stores { get; set; } = [];

}
