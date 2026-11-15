namespace GameExplorer.Api.Data.Entities;

public interface IListEntry
{
	int GameId { get; }
	DateTime AddedAt { get; }
}
