namespace GameExplorer.Shared.Models;

public class Studio
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public string Slug { get; set; } = "";
	public Image? Image { get; set; }
	public string? About { get; set; }
	public int? CountryId { get; set; }
	public DateOnly? StartDate;
	public int StatusId { get; set; }
	public string? Website { get; set; }
}