using System;
using System.Text.Json.Serialization;

namespace GameExplorer.Api.Services.IgdbDtos;

internal class IgdbStudio : IIgdbEntity
{
	public static string Url => "companies";
	public static string Fields => "fields id, name, slug, websites.*, description, logo.*, country, start_date, status;";
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("slug")]
	public string Slug { get; set; } = "";

	[JsonPropertyName("name")]
	public string Name { get; set; } = "";

	[JsonPropertyName("Websites")]
	public List<IgdbWebsite> Websites { get; set; } = [];
	
	[JsonPropertyName("description")]
	public string Description { get; set; } = "";
	
	[JsonPropertyName("logo")]
	public IgdbImage? Logo { get; set; }
	
	[JsonPropertyName("country")]
	public int? CountryId { get; set; }
	
	[JsonPropertyName("start_date")]
	public long StartDate { get; set; }
	
	[JsonPropertyName("status")]
	public int Status { get; set; }
}