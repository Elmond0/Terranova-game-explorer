using System.Text.Json.Serialization;
using GameExplorer.Api.Services.IgdbDtos;

namespace GameExplorer.Api.Services.IgdbDtos;

internal class IgdbGameDetails : IgdbGame
{
	[JsonPropertyName("summary")]
	public string? Description { get; set; }

	[JsonPropertyName("storyline")]
	public string? StoryLine { get; set; }

	[JsonPropertyName("involved_companies")]
	public List<IgdbStudio> Developers { get; set; } = [];
	
	[JsonPropertyName("involved_companies")]
	public List<IgdbStudio> Publishers { get; set; } = [];

	[JsonPropertyName("websites")]
	public List<IgdbWebsite> Websites { get; set; } = [];
	
	[JsonPropertyName("screenshots")]
	public List<IgdbImage> Screenshots { get; set; } = [];
}