namespace GameExplorer.Api.Data.Entities;

public class WishlistItem
{
	public int GameId { get; set; }
	public string UserId { get; set; } = "";
	public DateTime AddedAt { get; set; }
}
