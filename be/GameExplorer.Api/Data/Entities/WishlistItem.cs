namespace GameExplorer.Api.Data.Entities;

public class WishlistItem : IListEntry
{
	public int GameId { get; set; }
	public string UserId { get; set; } = "";
	public DateTime AddedAt { get; set; }
	public GameSnapshot Game { get; set; } = null!;
}
