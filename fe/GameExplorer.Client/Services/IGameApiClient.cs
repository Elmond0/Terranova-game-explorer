using GameExplorer.Shared.Models;

namespace GameExplorer.Client.Services;

public interface IGameApiClient
{
	Task<List<Game>> GetTopRatedAsync(CancellationToken cancellationToken = default);
	Task<List<Game>> GetRecentAsync(CancellationToken cancellationToken = default);
	Task<List<Game>> GetUpcomingAsync(CancellationToken cancellationToken = default);
	Task<PagedResult<Game>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default);
	Task<GameDetails?> GetGameAsync(int id, CancellationToken cancellationToken = default);
	Task<List<Genre>> GetGenresAsync(CancellationToken cancellationToken = default);
	Task<List<Platform>> GetPlatformsAsync(CancellationToken cancellationToken = default);

}
