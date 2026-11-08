using System.Text.Json.Serialization;
namespace GameExplorer.Api.Services.IgdbDtos;

public class IgdbItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}