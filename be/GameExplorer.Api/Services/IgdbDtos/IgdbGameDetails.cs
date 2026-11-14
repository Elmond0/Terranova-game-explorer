using System.Text.Json.Serialization;
using GameExplorer.Api.Services.IgdbDtos;

namespace GameExplorer.Api.Services.IgdbDtos;

internal class IgdbGameDetails : IgdbGame
{
	public new static string Fields => "fields id, name, slug, rating, first_release_date, cover.*, genres.*, platforms.*, summary, storyline, involved_companies.company.id, involved_companies.company.name, involved_companies.company.slug, involved_companies.company.websites.*, involved_companies.company.description, involved_companies.company.logo.*, involved_companies.company.country, involved_companies.company.start_date, involved_companies.company.status, involved_companies.developer, involved_companies.publisher  , websites.*, screenshots.*;";
	[JsonPropertyName("summary")]
	public string? Description { get; set; }

	[JsonPropertyName("storyline")]
	public string? StoryLine { get; set; }

	[JsonPropertyName("involved_companies")]
	public List<IgdbInvolvedCompany> Studios { get; set; } = [];

	[JsonPropertyName("websites")]
	public List<IgdbWebsite> Websites { get; set; } = [];
	
	[JsonPropertyName("screenshots")]
	public List<IgdbImage> Screenshots { get; set; } = [];
}