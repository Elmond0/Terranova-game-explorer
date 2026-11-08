using GameExplorer.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace GameExplorer.Client.Services;

public class GameApiClient : IGameApiClient
{
	private readonly HttpClient _http;

	public GameApiClient(HttpClient http)
	{
		_http = http;
	}

	public async Task<List<Game>> GetTopRatedAsync(CancellationToken cancellationToken = default)
	{
		var games = await GetAsync<List<Game>>("api/games/top-rated", cancellationToken);
		return games ?? [];
	}

	public async Task<List<Game>> GetRecentAsync(CancellationToken cancellationToken = default)
	{
		var games = await GetAsync<List<Game>>("api/games/recent", cancellationToken);
		return games ?? [];
	}

	public async Task<List<Game>> GetUpcomingAsync(CancellationToken cancellationToken = default)
	{
		var games = await GetAsync<List<Game>>("api/games/upcoming", cancellationToken);
		return games ?? [];
	}

	public async Task<PagedResult<Game>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default)
	{
		var page = await GetAsync<PagedResult<Game>>($"api/games?{BuildQueryString(filter)}", cancellationToken);
		return page ?? new PagedResult<Game>();
	}

	public async Task<GameDetails?> GetGameAsync(int id, CancellationToken cancellationToken = default)
	{
		var game = await GetAsync<GameDetails>($"api/games/{id}", cancellationToken);
		return game;
	}

	public async Task<List<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
	{
		var genres = await GetAsync<List<Genre>>("api/genres", cancellationToken);
		return genres ?? [];
	}

	public async Task<List<Platform>> GetPlatformsAsync(CancellationToken cancellationToken = default)
	{
		var platforms = await GetAsync<List<Platform>>("api/platforms", cancellationToken);
		return platforms ?? [];
	}

	public async Task<Studio?> GetStudioAsync(int id, CancellationToken cancellationToken = default)
	{
		var studio = await GetAsync<Studio>($"api/studios/{id}", cancellationToken);
		return studio;
	}

	public async Task<PagedResult<Game>> GetStudioGamesAsync(int id, int page, string? ordering, CancellationToken cancellationToken = default)
	{
		var url = $"api/studios/{id}/games?page={page}";
		if (!string.IsNullOrWhiteSpace(ordering))
			url += $"&ordering={Uri.EscapeDataString(ordering)}";
		var games = await GetAsync<PagedResult<Game>>(url, cancellationToken);
		return games ?? new PagedResult<Game>();
	}

	// 404 → null, annullamento chiesto dalla pagina → passa così com'è, qualsiasi altro errore → ApiException
	private async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken)
	{
		try
		{
			return await _http.GetFromJsonAsync<T>(url, cancellationToken);
		}
		catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
		{
			return default;
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new ApiException($"Chiamata a {url} fallita", ex);
		}
	}

	// Unico punto in cui SearchFilter diventa query string: nomi delle proprietà in camelCase,
	// liste con la chiave ripetuta (?genreIds=4&genreIds=3), come le legge [FromQuery] SearchFilter.
	// Se la Web API cambia i nomi dei parametri, si corregge solo qui.
	private static string BuildQueryString(SearchFilter filter)
	{
		List<string> parts = [];

		if (!string.IsNullOrWhiteSpace(filter.Query))
			parts.Add($"query={Uri.EscapeDataString(filter.Query)}");
		AddList(parts, "genreIds", filter.GenreIds);
		AddList(parts, "platformIds", filter.PlatformIds);
		if (filter.FromYear is not null)
			parts.Add($"fromYear={filter.FromYear}");
		if (filter.ToYear is not null)
			parts.Add($"toYear={filter.ToYear}");
		AddList(parts, "developerIds", filter.DeveloperIds);
		AddList(parts, "publisherIds", filter.PublisherIds);
		if (!string.IsNullOrWhiteSpace(filter.Ordering))
			parts.Add($"ordering={Uri.EscapeDataString(filter.Ordering)}");
		parts.Add($"page={filter.Page}");
		parts.Add($"pageSize={filter.PageSize}");

		return string.Join("&", parts);
	}

	// Una coppia chiave=valore per ogni id: [4, 3] → genreIds=4&genreIds=3
	private static void AddList(List<string> parts, string key, List<int> ids)
	{
		foreach (var id in ids)
			parts.Add($"{key}={id}");
	}
}
