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
	// private async Task<PagedResult<T>>	GetCachedPagedResultAsync<TRaw, T>( string path, int page, int pageSize, Func<TRaw, T> convert, HybridCacheEntryOptions options, CancellationToken ct )
	// {
	// 	return await _cache.GetOrCreateAsync(
	// 		path,
	// 		async cancel =>
	// 		{
	// 			RawgList<TRaw>? raw = await GetAsync<RawgList<TRaw>>(path, cancel);
	// 			if (raw is null)
	// 				return new PagedResult<T>();
	// 			return raw.InPage<TRaw, T>(page, pageSize, convert);
	// 		},
	// 		options,
	// 		cancellationToken: ct);
	// }
	//
		
		
	private async Task<List<T>>	GetCachedListAsync<TRaw, T>( string cacheKey, string url, string query, Func<TRaw, T> convert, HybridCacheEntryOptions options, CancellationToken ct)
	{
		return await cache.GetOrCreateAsync(
			cacheKey,
			async cancel =>
			{
				var raw = await igdb.QueryAsync<TRaw>(url, query, cancel);
				return raw.Select(convert).ToList();
			},
			options,
			cancellationToken: ct);
	}
	//
	// public async Task<PagedResult<Game>>	SearchAsync( SearchFilter filter, CancellationToken ct ) =>
	// 	await GetCachedPagedResultAsync<RawgGame, Game>(BuildSearchPath(filter), filter.Page, filter.PageSize, RawgConverter.InGame, RawgCache.Search, ct);
	//
	public async Task<GameDetails?> GetGameDetailsAsync( int id, CancellationToken ct ) =>
		await cache.GetOrCreateAsync( IgdbCache.GameKey(id), async cancel => await LoadGameDetailsAsync(id, cancel), IgdbCache.Details, cancellationToken: ct);
	
	private async Task<GameDetails?>	LoadGameDetailsAsync( int id, CancellationToken ct )
	{
		var raw = await igdb.QueryAsync<IgdbGameDetails>("games",$"{IgdbQueryFields.Details} where id = {id};", ct);
		return raw.Count == 0 ? null : raw.FirstOrDefault()!.ToDetails() ;
	}
	
	// internal async Task<List<Genre>?> GetGenresAsync( CancellationToken ct ) => 
	// 	await GetCachedListAsync<RawgItem, Genre>( "genres", "genres?page_size=40", RawgConverter.InGenre, RawgCache.Lookup, ct );
	//
	// internal async Task<List<Platform>?> GetPlatformsAsync( CancellationToken ct ) => 
	// 	await GetCachedListAsync<RawgItem, Platform>( "platforms", "platforms?page_size=40", RawgConverter.InPlatform, RawgCache.Lookup, ct );
	
	internal async Task<List<Game>>	GetTopRatedAsync( CancellationToken ct ) =>
		await GetCachedListAsync<IgdbGame, Game>( "top-rated", "games", $"{IgdbQueryFields.TopRated}", IgdbMapper.ToGame, IgdbCache.Home, ct );

	internal async Task<List<Game>>	GetRecentAsync( CancellationToken ct ) =>
		await GetCachedListAsync<IgdbGame, Game>( "recent", "games", $"{IgdbQueryFields.Recent}", IgdbMapper.ToGame, IgdbCache.Home, ct );

	internal async Task<List<Game>>	GetUpcomingAsync( CancellationToken ct ) =>
		await GetCachedListAsync<IgdbGame, Game>( "upcoming", "games", $"{IgdbQueryFields.Upcoming}", IgdbMapper.ToGame, IgdbCache.Home, ct );

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