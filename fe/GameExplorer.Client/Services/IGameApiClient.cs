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
	Task<Studio?> GetStudioAsync(int id, CancellationToken cancellationToken = default);
	Task<PagedResult<Game>> GetStudioGamesAsync(int id, int page, string? ordering, CancellationToken cancellationToken = default);
	Task<List<Game>> GetStudioUpcomingAsync(int id, CancellationToken cancellationToken = default);
	// TODO 1b: paginated "see all" lists, enabled once A's endpoints accept ?page= and are on main.
	// A separate method from the home one because the home needs a short list, "see all" needs pages.
	// Task<PagedResult<Game>> GetTopRatedPageAsync(int page, CancellationToken cancellationToken = default);
	// Task<PagedResult<Game>> GetRecentPageAsync(int page, CancellationToken cancellationToken = default);
	// Task<PagedResult<Game>> GetUpcomingPageAsync(int page, CancellationToken cancellationToken = default);
	Task<List<Game>> GetFavoritesAsync(CancellationToken cancellationToken = default);
	Task AddFavoriteAsync(int id, CancellationToken cancellationToken = default);
	Task RemoveFavoriteAsync(int id, CancellationToken cancellationToken = default);
	Task<List<Game>> GetWishlistAsync(CancellationToken cancellationToken = default);
	Task AddToWishlistAsync(int id, CancellationToken cancellationToken = default);
	Task RemoveFromWishlistAsync(int id, CancellationToken cancellationToken = default);
	Task<List<Game>> GetPlatformHighlightsAsync(int platformId, CancellationToken cancellationToken = default);
	Task<ProfileResponse> GetProfileAsync(CancellationToken cancellationToken = default);
	Task UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
}
