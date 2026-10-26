using System.ComponentModel.DataAnnotations;

namespace GameExplorer.Shared.Models;

public class StoreLink
{
	public Store? Store { get; set; }
	public string? Url { get; set; }
}