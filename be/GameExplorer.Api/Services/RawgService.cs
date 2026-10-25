using System.Globalization;
using System.Net;
using GameExplorer.Api.Services.Rawg;
using GameExplorer.Shared.Models;

namespace GameExplorer.Api.Services;

public class RawgService
{
	private readonly HttpClient	_http;
	private readonly string		_key;
	public RawgService( HttpClient http, IConfiguration config )
	{
		_http = http;
		_key = config["Rawg:ApiKey"]
			?? throw new InvalidOperationException("Missing API key.");
	}

	public async Task<GameDetails?>	GetGameDetailsAsync( int id, CancellationToken ct )
	{
		Task<List<string>?> shotsTask = GetScreenshotsAsync(id, ct);
		using HttpResponseMessage res = await _http.GetAsync($"games/{id}?key={_key}", ct);
		if (res.StatusCode == HttpStatusCode.NotFound)
			return null;
		if (!res.IsSuccessStatusCode)
			throw new RawgException(res.StatusCode, $"RAWG exited with code {(int)res.StatusCode}.");
		RawgGameDetails? raw = await res.Content.ReadFromJsonAsync<RawgGameDetails>(ct);
		List<string>? shots = await shotsTask;
		return RawgConverter.InDetails(raw!, shots);
	}

	public async Task<List<string>?> GetScreenshotsAsync( int id, CancellationToken ct )
	{
		List<string> shots = [];
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
			shots.AddRange(raw.Results.Where(s => s.Image is not null).Select(s => s.Image!));
			url = raw.Next;
		}
		return shots;
	}
}