namespace GameExplorer.Api.Data.Entities;

// Same shape as Favorite but its own class, so EF Core maps it to its own table.

public class WishlistItem : IListEntry
{
	public int GameId { get; set; }
	public int UserId { get; set; }
	public DateTime AddedAt { get; set; }
}
