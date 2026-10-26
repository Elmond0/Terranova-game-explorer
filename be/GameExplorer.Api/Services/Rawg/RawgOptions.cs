using System.ComponentModel.DataAnnotations;

public class RawgOptions
{
	[Required, MinLength(1)]
	public string ApiKey { get; set; } = "";
}