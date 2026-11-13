using Microsoft.Extensions.Caching.Hybrid;
using GameExplorer.Shared.Models;
namespace GameExplorer.Api.Services;

public class IgdbCache
{
    public static readonly HybridCacheEntryOptions Lookup = new() { Expiration = TimeSpan.FromHours(24), LocalCacheExpiration = TimeSpan.FromHours(24)};
    public static readonly HybridCacheEntryOptions Details = new() { Expiration = TimeSpan.FromHours(6), LocalCacheExpiration = TimeSpan.FromHours(6)};
    public static readonly HybridCacheEntryOptions Home = new() { Expiration = TimeSpan.FromHours(1), LocalCacheExpiration = TimeSpan.FromHours(1)};
    public static readonly HybridCacheEntryOptions Search = new() { Expiration = TimeSpan.FromMinutes(10), LocalCacheExpiration = TimeSpan.FromMinutes(10)};
    public static readonly HybridCacheEntryOptions Token = new() { Expiration = TimeSpan.FromDays(1), LocalCacheExpiration = TimeSpan.FromDays(1), Flags = HybridCacheEntryFlags.DisableDistributedCache};

    public static string GameKey( int id ) => $"game:{id}";
    public static string StudioKey( int id ) => $"studio:{id}";
    public static string StudioUpcomingKey( int id ) => $"studio:{id}:upcoming";
    public static string StudioGamesKey( int id ) => $"studio:{id}:games";
    public static string PlatformHighlightsKey( int id ) => $"platform:{id}";
    
    public const string TokenKey = "igdb:twitch-token";
    public static string SearchKey( (string query, string pagination, string sort) body )  => $"search:{body.query}:{body.pagination}:{body.sort}";
}