using System.ComponentModel.DataAnnotations;

namespace GameExplorer.Api.Services;

public class IgdbOptions
{
	[Required]
	public string ClientId { get; set; } = "";
	
	[Required]
	public string ClientSecret { get; set; } = "";
}