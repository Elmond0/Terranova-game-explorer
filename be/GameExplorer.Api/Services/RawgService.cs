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
		List<RawgScreenshot>? shots = await GetScreenshotsAsync(id, ct);
		if (raw is null)
			return null;
		return RawgConverter.InDetails(raw!, shots);
	}

	internal async Task<List<RawgScreenshot>?> GetScreenshotsAsync( int id, CancellationToken ct )
	{
		List<RawgScreenshot> shots = [];
		string? url = $"games/{id}/screenshots?page_size=40";
		
		while (url is not null)
		{
			RawgList<RawgScreenshot>? raw = await GetAsync<RawgList<RawgScreenshot>>(url, ct);
			if (raw is null)
				break ;
			shots = (raw.Results ?? [])
				.Where(r => r is not null && r.IsDeleted is not true)
				.ToList();
			url = raw.Next;
		}
		return shots;
	}
}