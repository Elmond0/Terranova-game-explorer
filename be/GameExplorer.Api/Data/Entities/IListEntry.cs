namespace GameExplorer.Api.Data.Entities;

// What favorites and wishlist have in common: one generic method in MeEndpoints reads both lists instead of two copies.

public interface IListEntry
{
	int GameId { get; }
	DateTime AddedAt { get; }
}
