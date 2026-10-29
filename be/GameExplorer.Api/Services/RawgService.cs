using System.Text.Json;
using System.Net;
using GameExplorer.Api.Services.Rawg;
using GameExplorer.Shared.Models;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using GameExplorer.Api.Errors;

namespace GameExplorer.Api.Services;

public class RawgService
{
	private readonly HttpClient	_http;
	private readonly string		_key;
	private readonly ILogger<RawgService> _logger;

    public RawgService(
        HttpClient http,
        IOptions<RawgOptions> opt,
        ILogger<RawgService> logger )
    {
        _http = http;
        _key = opt.Value.ApiKey;
        _logger = logger;
    }

	private async Task<T?>  GetAsync<T>( string path, CancellationToken ct )
    {
        char sep = path.Contains('?') ? '&' : '?';
        string url = $"{path}{sep}key={_key}";

        try
        {
			_logger.LogInformation("RAWG request: {Url}", url.Replace(_key ?? "", "***"));
			_logger.LogInformation("Key loaded: {Loaded}, length {Len}", !string.IsNullOrEmpty(_key), _key?.Length);
            using HttpResponseMessage res = await _http.GetAsync(url, ct);
            if (res.StatusCode == HttpStatusCode.NotFound)
                return default;
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<T>(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw ;
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException)
        {
            throw new RawgException("Rawg does not respond", e);
        }
    }

	public async Task<GameDetails?>	GetGameDetailsAsync( int id, CancellationToken ct )
	{
		RawgGameDetails? raw = await GetAsync<RawgGameDetails>($"games/{id}", ct);
		List<string?>? shots = await GetScreenshotsAsync(id, ct);
		if (raw is null)
			return null;
		return RawgConverter.InDetails(raw!, shots);
	}

	internal async Task<List<string?>?> GetScreenshotsAsync( int id, CancellationToken ct )
	{
		List<string?> shots = [];
		string? url = $"games/{id}/screenshots?page_size=40";
		
		while (url is not null)
		{
			RawgList<RawgScreenshot>? raw = await GetAsync<RawgList<RawgScreenshot>>(url, ct);
			if (raw is null)
				break ;
			shots = (raw.Results ?? [])
				.Where(r => r is not null && r.IsDeleted is not true)
				.Select(r => r!.Image)
				.ToList();
			url = raw.Next;
		}
		return shots;
	}

	internal async Task<List<Genre>?> GetGenresAsync( CancellationToken ct )
	{
		List<Genre> genres = [];
		string? url = $"genres?page_size=40";
		RawgList<RawgItem>? raw = await GetAsync<RawgList<RawgItem>>(url, ct);
		if (raw is null)
			return [];
		genres = (raw.Results ?? [])
			.Where(r => r is not null)
			.Select(r => r!.InGenre())
			.ToList();
		return genres;
	}

	internal async Task<List<Platform>?> GetPlatformsAsync( CancellationToken ct )
	{
		List<Platform> platforms = [];
		string? url = $"genres?page_size=40";
		RawgList<RawgItem>? raw = await GetAsync<RawgList<RawgItem>>(url, ct);
		if (raw is null)
			return [];
		platforms = (raw.Results ?? [])
			.Where(r => r is not null)
			.Select(r => r!.InPlatform())
			.ToList();
		return platforms;
	}

	internal async Task<List<Game>>	GetTopRatedAsync( CancellationToken ct )
	{
		RawgList<RawgGame>? raw = await GetAsync<RawgList<RawgGame>>($"games?ordering=-rating", ct);
		if (raw is null)
			return [];
		return raw.Results
			.Where(r => r is not null)
			.Select(r => r!.InGame())
			.ToList();
	}

	internal async Task<List<Game>>	GetRecentAsync( CancellationToken ct )
	{
		RawgList<RawgGame>? raw = await GetAsync<RawgList<RawgGame>>($"games?ordering=-released&dates=2026-09-29,2026-10-29&ordering=-released", ct);
		if (raw is null)
			return [];
		return raw.Results
			.Where(r => r is not null)
			.Select(r => r!.InGame())
			.ToList();
	}

	internal async Task<List<Game>>	GetUpcomingAsync( CancellationToken ct )
	{
		RawgList<RawgGame>? raw = await GetAsync<RawgList<RawgGame>>($"games?ordering=-released&dates=2026-10-29,2027-10-29&ordering=-released", ct);
		if (raw is null)
			return [];
		return raw.Results
			.Where(r => r is not null)
			.Select(r => r!.InGame())
			.ToList();
	}

}