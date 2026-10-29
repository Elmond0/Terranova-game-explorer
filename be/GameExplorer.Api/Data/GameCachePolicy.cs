using GameExplorer.Api.Data.Entities;

namespace GameExplorer.Api.Data;

/// <summary> How long a cached game is valid before asking RAWG again. </summary>
public static class GameCachePolicy
{
	static readonly TimeSpan ReleasedMaxAge = TimeSpan.FromDays(7);
	static readonly TimeSpan UpcomingMaxAge = TimeSpan.FromDays(1);

	public static bool IsFresh(GameCacheEntry entry, DateTime nowUtc)
	{
		DateOnly today = DateOnly.FromDateTime(nowUtc);
		bool released = entry.ReleaseDate.HasValue && entry.ReleaseDate.Value <= today;
		TimeSpan maxAge = released ? ReleasedMaxAge : UpcomingMaxAge;
		return nowUtc - entry.UpdatedAt < maxAge;
	}
}
