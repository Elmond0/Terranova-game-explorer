using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.Rawg;

internal class RawgScreenshot
{
	[JsonPropertyName("id")]
	public int Id { get; set; }
	
	[JsonPropertyName("image")]
	public string? Image { get; set; }

	[JsonPropertyName("is_deleted")]
	public bool IsDeleted { get; set; }
}