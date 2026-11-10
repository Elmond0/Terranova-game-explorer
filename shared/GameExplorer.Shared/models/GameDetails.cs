namespace GameExplorer.Shared.Models;

public class GameDetails : Game
{
	public string? Description { get; set; }
	public string? Storyline { get; set; }
	public string? Website { get; set; }
	public List<Image> Screenshots { get; set; } = [];
	public List<Studio> Developers { get; set; } = [];
}
