using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.IgdbDtos;

internal class IgdbImage
{
	[JsonPropertyName("id")]
	public int Id { get; set; }
	
	[JsonPropertyName("url")]
	public string? Path { get; set; }

	[JsonPropertyName("height")]
	public int Height { get; set; }
	
	[JsonPropertyName("width")]
	public int Width { get; set; }
}