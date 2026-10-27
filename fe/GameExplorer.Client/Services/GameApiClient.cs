using GameExplorer.Shared.Models;
using System.Net.Http.Json;

namespace GameExplorer.Client.Services;

public class GameApiClient : IGameApiClient
{
	private readonly HttpClient _http;

	public GameApiClient(HttpClient http)
	{
		_http = http;
	}

	public async Task<List<Game>> GetTopRatedAsync()
	{
		var games = await _http.GetFromJsonAsync<List<Game>>("api/games/top-rated");
		return games ?? [];
	}

	public async Task<List<Game>> GetRecentAsync()
	{
		var games = await _http.GetFromJsonAsync<List<Game>>("api/games/recent");
		return games ?? [];
	}

	public async Task<List<Game>> GetUpcomingAsync()
	{
		var games = await _http.GetFromJsonAsync<List<Game>>("api/games/upcoming");
		return games ?? [];
	}
}
