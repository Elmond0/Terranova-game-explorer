using System.Text;
using GameExplorer.Api.Services.IgdbDtos;
using GameExplorer.Shared.Models;
using Microsoft.Extensions.Caching.Hybrid;

namespace GameExplorer.Api.Services;

public class GameService( IgdbClient igdb,  HybridCache cache )
{
	private const int MaxQueryLength = 100;

	private static string SanitizeSearchText(string input)
	{
		var sb = new StringBuilder(Math.Min(input.Length, MaxQueryLength));
		foreach (var c in input.Trim())
		{
			if (c is '"' or '\\' or ';' || char.IsControl(c))
				continue;
			sb.Append(c);
			if (sb.Length == MaxQueryLength)
				break;
		}
		return sb.ToString().Trim();
	}
	
	private static (string query, string pagination, string sort) BuildSearchQuery( SearchFilter filter )
	{
		var query = "";
		
		var where = new List<string>();
		if (!string.IsNullOrWhiteSpace(filter.Query))
		{
			var safe = SanitizeSearchText(filter.Query);
			if (safe.Length > 0)
				where.Add($"(name ~ *\"{safe}\"* | alternative_names.name ~ *\"{safe}\"*)");
		}
		if (filter.GenreIds.Count > 0)
			where.Add($"genres = [{string.Join(',', filter.GenreIds)}]");
		if (filter.PlatformIds.Count > 0)
			where.Add($"platforms = [{string.Join(',', filter.PlatformIds)}]");
		if (filter.DeveloperIds.Count > 0)
			where.Add($"involved_companies.company = [{string.Join(',', filter.DeveloperIds)}]");
		if (filter.FromYear is not null)
		{
			var from = filter.FromYear.Value;
			where.Add($"first_release_date >= {IgdbMapper.ToUnix(new DateOnly(from, 1, 1))}");
		}
		if (filter.ToYear is not null)
		{
			var to = filter.ToYear.Value;
			where.Add($"first_release_date < {IgdbMapper.ToUnix(new DateOnly(to + 1, 1, 1))}");
		}
		where.Add($"game_type = (0,1,2,4,8,9,10,11)");
		where.Add($"version_parent = null");
		query += $"where {string.Join(" & ", where)};";
		
		var offset = (filter.Page - 1) * filter.PageSize;
		var pagination = $"limit {filter.PageSize};" +
		                 $"offset {offset};";
		
		var field = "total_rating_count";
		var desc = true;
		if (!string.IsNullOrWhiteSpace(filter.Ordering))
		{
			desc = filter.Ordering.StartsWith('-');
			var criteria = filter.Ordering.TrimStart('-');
			field = criteria switch
			{
				"name"       => "name",
				"released"   => "first_release_date",
				"created"    => "created_at",
				"updated"    => "updated_at",
				"rating"     => "rating",
				"popularity" => "total_rating_count",
				"added"		 => "created_at",
				_            => "total_rating_count"
			};
		}
		var sort = $"sort {field} ";
		sort += (desc ? "desc;" : "asc;");

		return (query, pagination, sort);
	}
	
	private async Task<PagedResult<T>>	GetCachedPagedResultAsync<TRaw, T>( string cacheKey, (string query, string pagination, string sort) body, int page, int pageSize, Func<TRaw, T> convert, HybridCacheEntryOptions options, CancellationToken ct )
		where TRaw : IIgdbEntity =>
		await cache.GetOrCreateAsync(
			cacheKey,
			async cancel =>
			{
				var itemTask = igdb.QueryAsync<TRaw>(TRaw.Fields + body.query + body.pagination + body.sort, cancel);
				var countTask = igdb.QueryCountAsync<TRaw>(TRaw.Fields + body.query, cancel);
				await Task.WhenAll(itemTask, countTask);
				
				var total = countTask.Result;
				return new PagedResult<T>
				{
					Items = itemTask.Result
						.Select(convert).ToList(),
					Total = total,
					Page = page,
					PageSize = pageSize
				};
			},
			options,
			cancellationToken: ct);
	
	private async Task<List<T>>	GetCachedListAsync<TRaw, T>( string cacheKey, (string query, string pagination, string sort) body, Func<TRaw, T> convert, HybridCacheEntryOptions options, CancellationToken ct)
		where TRaw: IIgdbEntity =>
		await cache.GetOrCreateAsync(
			cacheKey,
			async cancel =>
			{
				var raw = await igdb.QueryAsync<TRaw>(TRaw.Fields + body.query + body.pagination + body.sort, cancel);
				return raw.Select(convert).ToList();
			},
			options,
			cancellationToken: ct);

	public async Task<PagedResult<Game>> SearchAsync(SearchFilter filter, CancellationToken ct)
	{
		var body = BuildSearchQuery(filter);
		return await GetCachedPagedResultAsync<IgdbGame, Game>(IgdbCache.SearchKey(body), body, filter.Page, filter.PageSize, IgdbMapper.ToGame, IgdbCache.Search, ct);
	}
	
	public async Task<GameDetails?> GetGameDetailsAsync( int id, CancellationToken ct ) =>
		await cache.GetOrCreateAsync(
			IgdbCache.GameKey(id), 
			async cancel => 
				await LoadGameDetailsAsync(id, cancel), IgdbCache.Details, cancellationToken: ct);
	
	private async Task<GameDetails?>	LoadGameDetailsAsync( int id, CancellationToken ct )
	{
		var raw = await igdb.QueryAsync<IgdbGameDetails>($"{IgdbGameDetails.Fields} where id = {id};", ct);
		return raw.Count == 0 ? null : raw.FirstOrDefault()!.ToDetails() ;
	}
	
	internal async Task<List<Genre>?> GetGenresAsync( CancellationToken ct ) => 
		await GetCachedListAsync<IgdbGenre, Genre>( "genres", IgdbQueryFields.Genres(), IgdbMapper.ToGenre, IgdbCache.Lookup, ct );
	
	private static readonly int[] MainPlatformIds = { 6, 167, 169, 130, 48, 49 }; // PC, PS5, Xbox Series, Switch, PS4, Xbox One

	internal async Task<List<Platform>?> GetPlatformsAsync(CancellationToken ct)
	{
		var platforms = await GetCachedListAsync<IgdbPlatform, Platform>( "platforms", IgdbQueryFields.Platforms(), IgdbMapper.ToPlatform, IgdbCache.Lookup, ct );
		var rank = MainPlatformIds
			.Select((id, index) => (id, index))
			.ToDictionary(x => x.id, x => x.index);
		
		return platforms
			.OrderBy(p => rank.TryGetValue(p.Id, out var i) ? i : int.MaxValue)
			.ThenBy(p => p.Name)                                             
			.ToList();
	}

	internal async Task<List<Game>> GetPlatformHighlightsAsync(int id, CancellationToken ct) =>
		await GetCachedListAsync<IgdbGame, Game>(IgdbCache.PlatformHighlightsKey(id),
			IgdbQueryFields.PlatformHighlights(id), IgdbMapper.ToGame, IgdbCache.Home, ct);
	
	internal async Task<List<Game>>	GetHomeTopRatedAsync( CancellationToken ct ) =>
		await GetCachedListAsync<IgdbGame, Game>( "home:top-rated", IgdbQueryFields.TopRated(), IgdbMapper.ToGame, IgdbCache.Home, ct );
	
	internal async Task<PagedResult<Game>>	GetTopRatedAsync( int page, int pageSize, CancellationToken ct ) =>
		await GetCachedPagedResultAsync<IgdbGame, Game>( IgdbCache.TopRatedKey(page, pageSize), IgdbQueryFields.TopRated(page, pageSize), page, pageSize, IgdbMapper.ToGame, IgdbCache.Home, ct );

	internal async Task<List<Game>>	GetHomeRecentAsync( CancellationToken ct ) =>
		await GetCachedListAsync<IgdbGame, Game>( "home:recent",  IgdbQueryFields.Recent(), IgdbMapper.ToGame, IgdbCache.Home, ct );
	
	internal async Task<PagedResult<Game>>	GetRecentAsync( int page, int pageSize, CancellationToken ct ) =>
		await GetCachedPagedResultAsync<IgdbGame, Game>( IgdbCache.RecentKey(page, pageSize), IgdbQueryFields.Recent(page, pageSize, false), page, pageSize, IgdbMapper.ToGame, IgdbCache.Home, ct );

	internal async Task<List<Game>>	GetHomeUpcomingAsync( CancellationToken ct ) =>
		await GetCachedListAsync<IgdbGame, Game>( "home:upcoming", IgdbQueryFields.Upcoming(), IgdbMapper.ToGame, IgdbCache.Home, ct );
	
	internal async Task<PagedResult<Game>>	GetUpcomingAsync( int page, int pageSize, CancellationToken ct ) =>
		await GetCachedPagedResultAsync<IgdbGame, Game>( IgdbCache.UpcomingKey(page, pageSize), IgdbQueryFields.Upcoming(page, pageSize, false), page, pageSize, IgdbMapper.ToGame, IgdbCache.Home, ct );

	internal async Task<Studio?> GetStudioDetailsAsync(int id, CancellationToken ct) =>
		await cache.GetOrCreateAsync(
			IgdbCache.StudioKey(id),
			async cancel =>
			{
				var raw = await igdb.QueryAsync<IgdbStudio>($"{IgdbStudio.Fields} where id = {id};", cancel);
				return raw.Count == 0 ? null : raw.FirstOrDefault()!.ToStudio();
			},
			IgdbCache.Details,
			cancellationToken: ct);


	internal async Task<List<Game>?> GetStudioUpcomingAsync(int id, CancellationToken ct) =>
		await GetCachedListAsync<IgdbGame, Game>(IgdbCache.StudioUpcomingKey(id),
			IgdbQueryFields.StudioUpcoming(id), IgdbMapper.ToGame, IgdbCache.Details, ct);
	
	internal async Task<PagedResult<Game>> GetStudioGamesAsync(int id, int page, int pageSize, CancellationToken ct) =>
		await GetCachedPagedResultAsync<IgdbGame, Game>(IgdbCache.StudioGamesKey(id, page, pageSize),
			IgdbQueryFields.StudioGames(id, page, pageSize), page, pageSize, IgdbMapper.ToGame, IgdbCache.Details, ct);
}