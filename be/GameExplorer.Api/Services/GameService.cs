using System.Text.Json;
using System.Net;
using System.Linq;
using GameExplorer.Api.Services.IgdbDtos;
using GameExplorer.Shared.Models;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using GameExplorer.Api.Errors;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Polly;
using Polly.Timeout;
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace GameExplorer.Api.Services;

public class GameService( IgdbClient igdb,  HybridCache cache )
{
	private static (string query, string pagination, string sort) BuildSearchQuery( SearchFilter filter )
	{
		var query = "";

		var search = "";
		var where = new List<string>();
		if (!string.IsNullOrWhiteSpace(filter.Query))
		{
			var trimmed = filter.Query.Trim();
			if (string.IsNullOrWhiteSpace(filter.Ordering))
				search = $"search \"{trimmed}\";";
			else
				where.Add($"(name ~ *\"{trimmed}\"* | alternative_names.name ~ *\"{trimmed}\"*)");
		}
		if (filter.GenreIds.Count > 0)
			where.Add($"genres = [{string.Join(',', filter.GenreIds)}]");
		if (filter.PlatformIds.Count > 0)
			where.Add($"platforms = [{string.Join(',', filter.PlatformIds)}]");
		if (filter.DeveloperIds.Count > 0)
			where.Add($"involved_companies.company = [{string.Join(',', filter.DeveloperIds)}] & involved_companies.developer = true");
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
		query += $"{search}";
		if (where.Count > 0)
			query += $"where {string.Join(" & ", where)};";
		
		var offset = (filter.Page - 1) * filter.PageSize;
		var pagination = $"limit {filter.PageSize};" +
		                 $"offset {offset};";
		
		var sort = "";
		if (!string.IsNullOrWhiteSpace(filter.Ordering))
		{
			sort = "sort ";
			var desc = filter.Ordering.StartsWith('-');
			var criteria = filter.Ordering.TrimStart('-');
			switch (criteria)
			{
				case "name":
					sort += $"name ";
					break;
				case "released":
					sort += $"first_release_date ";
					break;
				case "created":
					sort += $"created_at ";
					break;
				case "updated":
					sort += $"updated_at ";
					break;
				case "rating":
					sort += $"rating ";
					break;
			}
			sort += desc ? "desc;" : "asc;";
		}

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
	
	private async Task<List<T>>	GetCachedListAsync<TRaw, T>( string cacheKey, string query, Func<TRaw, T> convert, HybridCacheEntryOptions options, CancellationToken ct)
		where TRaw: IIgdbEntity =>
		await cache.GetOrCreateAsync(
			cacheKey,
			async cancel =>
			{
				var raw = await igdb.QueryAsync<TRaw>(TRaw.Fields + query, cancel);
				return raw.Select(convert).ToList();
			},
			options,
			cancellationToken: ct);

	public async Task<PagedResult<Game>> SearchAsync(SearchFilter filter, CancellationToken ct)
	{
		var body = BuildSearchQuery(filter);
		return await GetCachedPagedResultAsync<IgdbGame, Game>($"search:{body}", body, filter.Page, filter.PageSize, IgdbMapper.ToGame, IgdbCache.Search, ct);
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
	
	// internal async Task<List<Genre>?> GetGenresAsync( CancellationToken ct ) => 
	// 	await GetCachedListAsync<IgdbItem, Genre>( "genres", "genres", "", IgdbMapper.ToGenre, IgdbCache.Lookup, ct );
	//
	// internal async Task<List<Platform>?> GetPlatformsAsync( CancellationToken ct ) => 
	// 	await GetCachedListAsync<IgdbItem, Platform>( "platforms", "games", "", IgdbMapper.ToPlatform, IgdbCache.Lookup, ct );
	
	internal async Task<List<Game>>	GetTopRatedAsync( CancellationToken ct ) =>
		await GetCachedListAsync<IgdbGame, Game>( "top-rated", IgdbQueryFields.TopRated, IgdbMapper.ToGame, IgdbCache.Home, ct );

	internal async Task<List<Game>>	GetRecentAsync( CancellationToken ct ) =>
		await GetCachedListAsync<IgdbGame, Game>( "recent",  IgdbQueryFields.Recent, IgdbMapper.ToGame, IgdbCache.Home, ct );

	internal async Task<List<Game>>	GetUpcomingAsync( CancellationToken ct ) =>
		await GetCachedListAsync<IgdbGame, Game>( "upcoming", IgdbQueryFields.Upcoming, IgdbMapper.ToGame, IgdbCache.Home, ct );

// 	internal async Task<List<string?>?> GetScreenshotsAsync( int id, CancellationToken ct ) =>
// 		await _cache.GetOrCreateAsync( RawgCache.ScreenshotsKey(id), async cancel => await LoadScreenshotsAsync(id, cancel), RawgCache.Details, cancellationToken: ct);
//
// 	private async Task<List<string?>?> LoadScreenshotsAsync( int id, CancellationToken ct )
// 	{
// 		List<string?> shots = [];
// 		string? url = $"games/{id}/screenshots?page_size=40";
// 		
// 		while (url is not null)
// 		{
// 			RawgList<RawgScreenshot>? raw = await GetAsync<RawgList<RawgScreenshot>>(url, ct);
// 			if (raw is null)
// 				break ;
// 			shots.AddRange((raw.Results ?? [])
// 				.Where(r => r is not null && r.IsDeleted is not true)
// 				.Select(r => r!.Image));
// 			url = raw.Next;
// 		}
// 		return shots;
// 	}
// }
}