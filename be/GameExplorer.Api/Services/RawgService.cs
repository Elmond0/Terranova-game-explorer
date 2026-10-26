using System.Globalization;
using System.Net;
using GameExplorer.Api.Services.Rawg;
using GameExplorer.Shared.Models;
using Microsoft.Extensions.Options;

namespace GameExplorer.Api.Services;

public class RawgService
{
	private readonly HttpClient	_http;
	private readonly string		_key;
	public RawgService( HttpClient http, IOptions<RawgOptions> opt )
	{
		_http = http;
		_key = opt.Value.ApiKey;
	}

	public async Task<GameDetails?>	GetGameDetailsAsync( int id, CancellationToken ct )
	{
		Task<List<RawgScreenshot>?> shotsTask = GetScreenshotsAsync(id, ct);
		using HttpResponseMessage res = await _http.GetAsync($"games/{id}?key={_key}", ct);
		if (res.StatusCode == HttpStatusCode.NotFound)
			return null;
		if (!res.IsSuccessStatusCode)
			throw new RawgException(res.StatusCode, $"RAWG exited with code {(int)res.StatusCode}.");
		RawgGameDetails? raw = await res.Content.ReadFromJsonAsync<RawgGameDetails>(ct);
		List<RawgScreenshot>? shots = await shotsTask;
		return RawgConverter.InDetails(raw!, shots);
	}

	internal async Task<List<RawgScreenshot>?> GetScreenshotsAsync( int id, CancellationToken ct )
	{
		List<RawgScreenshot> shots = [];
		string? url = $"games/{id}/screenshots?key={_key}&page_size=40";
		
		while (url is not null)
		{
			using HttpResponseMessage res = await _http.GetAsync(url, ct);

			if (res.StatusCode == HttpStatusCode.NotFound)
				return null;
			if (!res.IsSuccessStatusCode)
				throw new RawgException(res.StatusCode, $"RAWG exited with code {(int)res.StatusCode}.");

			RawgList<RawgScreenshot>? raw = await res.Content.ReadFromJsonAsync<RawgList<RawgScreenshot>>(ct);
			if (raw is null)
				break ;
			shots.AddRange(raw.Results
				.Where(r => r.IsDeleted is not true));
			url = raw.Next;
		}
		return shots;
	}
}