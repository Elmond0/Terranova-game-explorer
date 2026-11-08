using System.Text.Json.Serialization;
namespace GameExplorer.Api.Services.IgdbDtos;

public class IgdbGenre : IIgdbEntity
{
	public static string Url => "genres";
	public static string Fields => "fields id, name, slug;";

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}