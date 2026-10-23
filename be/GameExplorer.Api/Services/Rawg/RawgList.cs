using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.Rawg;

internal class RawgList<T>
{
	[JsonPropertyName("count")]
	public int Count { get; set; }

	[JsonPropertyName("next")]
	public string? Next { get; set; }
	
	[JsonPropertyName("previous")]
	public string? Previous { get; set; }

	[JsonPropertyName("results")]
	public List<T> Results { get; set; } = [];
}