using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.Rawg;

internal class RawgPlatformWrapper
{
	[JsonPropertyName("platform")]
	public RawgItem? Platform { get; set; }
}