namespace GameExplorer.Api.Data.Entities;

// Only the IGDB id of the game is saved, not its data: name, cover and rating are read from IGDB, so they never go out of date.
// AddedAt lets the list be sorted by when the game was added.

public class Favorite : IListEntry
{
	public int GameId { get; set; }
	public int UserId { get; set; }
	public DateTime AddedAt { get; set; }
}
