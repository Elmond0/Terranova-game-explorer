using Microsoft.Extensions.Caching.Hybrid;

namespace GameExplorer.Api.Services.Rawg;

internal static class RawgCache
{
	public static readonly HybridCacheEntryOptions Lookup = new() { Expiration = TimeSpan.FromHours(24), LocalCacheExpiration = TimeSpan.FromHours(24)};
	public static readonly HybridCacheEntryOptions Details = new() { Expiration = TimeSpan.FromHours(6), LocalCacheExpiration = TimeSpan.FromHours(6)};
	public static readonly HybridCacheEntryOptions Home = new() { Expiration = TimeSpan.FromHours(1), LocalCacheExpiration = TimeSpan.FromHours(1)};
	public static readonly HybridCacheEntryOptions Search = new() { Expiration = TimeSpan.FromMinutes(10), LocalCacheExpiration = TimeSpan.FromMinutes(10)};

	public static string GameKey( int id ) => $"game:{id}";
	
}