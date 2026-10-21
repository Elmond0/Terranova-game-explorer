using System.Security.Cryptography.X509Certificates;

namespace GameExplorer.Shared.Models;
public class GameDetails : Game
{
	public string? About { get; set; }
	public string? Website { get; set; }
	public List<string>? Screenshots { get; set; } = [];
	public List<Studio> Developers { get; set; } = [];
	public List<Studio> Publishers { get; set; } = [];
	public List<Store> WhereToBuy { get; set; } = [];
	public string AgeRating { get; set; } = "Not Rated";

}