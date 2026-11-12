using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.IgdbDtos;

internal class IgdbInvolvedCompany
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("company")]
    public IgdbStudio? Studio { get; set; }
    [JsonPropertyName("developer")]
    public bool Developer { get; set; }
    [JsonPropertyName("publisher")]
    public bool Publisher { get; set; }
}