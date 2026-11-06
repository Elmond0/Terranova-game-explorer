using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.IgdbDtos;

internal class IgdbList<T>
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