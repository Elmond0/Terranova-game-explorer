using GameExplorer.Shared.Models;

namespace GameExplorer.Client.Services;

public interface IGameApiClient
{
	Task<List<Game>>	GetTopRatedAsync();
	Task<List<Game>>	GetRecentAsync();
	Task<List<Game>>	GetUpcomingAsync();
}
