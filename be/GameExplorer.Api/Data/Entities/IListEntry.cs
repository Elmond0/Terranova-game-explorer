namespace GameExplorer.Api.Data.Entities;

public interface IListEntry
{
	DateTime AddedAt { get; }
	GameSnapshot Game { get; }
}
