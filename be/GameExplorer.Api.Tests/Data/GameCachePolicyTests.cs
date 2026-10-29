using System.Globalization;
using GameExplorer.Api.Data;
using GameExplorer.Api.Data.Entities;

namespace GameExplorer.Api.Tests.Data;

public class GameCachePolicyTests
{
	static readonly DateTime Now = new(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc);

	[Theory]
	[InlineData("2020-01-01", 6 * 24, true)]   // released, saved 6 days ago
	[InlineData("2020-01-01", 8 * 24, false)]  // released, saved 8 days ago
	[InlineData("2027-06-01", 23, true)]       // not released yet, saved 23 hours ago
	[InlineData("2027-06-01", 25, false)]      // not released yet, saved 25 hours ago
	[InlineData(null, 23, true)]               // no release date: 1-day rule
	[InlineData(null, 25, false)]
	public void IsFresh_uses_7_days_if_released_else_1_day(string? releaseDate, int ageHours, bool expected)
	{
		GameCacheEntry entry = new()
		{
			ReleaseDate = releaseDate is null
					? null
					: DateOnly.ParseExact(releaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
			UpdatedAt = Now.AddHours(-ageHours),
		};

		bool fresh = GameCachePolicy.IsFresh(entry, Now);

		Assert.Equal(expected, fresh);
	}
}
