namespace GameExplorer.Api.Data.Entities;

public class WishlistItem : IListEntry
{
	public int GameId { get; set; }
	public int UserId { get; set; }
	public DateTime AddedAt { get; set; }
}
