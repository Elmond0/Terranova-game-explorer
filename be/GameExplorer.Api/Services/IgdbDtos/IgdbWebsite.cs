using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.IgdbDtos;

internal class IgdbWebsite
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("url")]
    public string? Url { get; set; } = "";
    [JsonPropertyName("type")]
    public int Type { get; set; }
}