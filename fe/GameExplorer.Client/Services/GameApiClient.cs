using GameExplorer.Client.Services.Auth;
using GameExplorer.Shared.Models;
using System.Net;
using System.Net.Http.Json;

namespace GameExplorer.Client.Services;

public class GameApiClient : IGameApiClient
{
	private readonly HttpClient _http;
	private readonly CookieAuthenticationStateProvider _auth;

	public GameApiClient(HttpClient http, CookieAuthenticationStateProvider auth)
	{
		_http = http;
		_auth = auth;
	}

	public async Task<List<Game>> GetTopRatedAsync(CancellationToken cancellationToken = default)
	{
		var games = await GetAsync<List<Game>>("api/home/top-rated", false, cancellationToken);
		return games ?? [];
	}

	public async Task<List<Game>> GetRecentAsync(CancellationToken cancellationToken = default)
	{
		var games = await GetAsync<List<Game>>("api/home/recent", false, cancellationToken);
		return games ?? [];
	}

	public async Task<List<Game>> GetUpcomingAsync(CancellationToken cancellationToken = default)
	{
		var games = await GetAsync<List<Game>>("api/home/upcoming", false, cancellationToken);
		return games ?? [];
	}

	public Task<PagedResult<Game>> GetTopRatedPageAsync(int page, CancellationToken cancellationToken = default) =>
		GetGamesPageAsync("top-rated", page, cancellationToken);

	public Task<PagedResult<Game>> GetRecentPageAsync(int page, CancellationToken cancellationToken = default) =>
		GetGamesPageAsync("recent", page, cancellationToken);

	public Task<PagedResult<Game>> GetUpcomingPageAsync(int page, CancellationToken cancellationToken = default) =>
		GetGamesPageAsync("upcoming", page, cancellationToken);

	private async Task<PagedResult<Game>> GetGamesPageAsync(string list, int page, CancellationToken cancellationToken)
	{
		var games = await GetAsync<PagedResult<Game>>($"api/games/{list}?page={page}", false, cancellationToken);
		return games ?? new PagedResult<Game>();
	}
	public async Task<List<Game>> GetPlatformHighlightsAsync(int platformId, CancellationToken cancellationToken = default)
	{
		var games = await GetAsync<List<Game>>($"api/platforms/{platformId}/highlights", false, cancellationToken);
		return games ?? [];
	}

	public async Task<PagedResult<Game>> SearchAsync(SearchFilter filter, CancellationToken cancellationToken = default)
	{
		var page = await GetAsync<PagedResult<Game>>($"api/games?{BuildQueryString(filter)}", false, cancellationToken);
		return page ?? new PagedResult<Game>();
	}

	public async Task<GameDetails?> GetGameAsync(int id, CancellationToken cancellationToken = default)
	{
		var game = await GetAsync<GameDetails>($"api/games/{id}", true, cancellationToken);
		return game;
	}

	public async Task<List<Genre>> GetGenresAsync(CancellationToken cancellationToken = default)
	{
		var genres = await GetAsync<List<Genre>>("api/genres", false, cancellationToken);
		return genres ?? [];
	}

	public async Task<List<Platform>> GetPlatformsAsync(CancellationToken cancellationToken = default)
	{
		var platforms = await GetAsync<List<Platform>>("api/platforms", false, cancellationToken);
		return platforms ?? [];
	}

	public async Task<Studio?> GetStudioAsync(int id, CancellationToken cancellationToken = default)
	{
		var studio = await GetAsync<Studio>($"api/studios/{id}", true, cancellationToken);
		return studio;
	}

	public async Task<PagedResult<Game>> GetStudioGamesAsync(int id, int page, string? ordering, CancellationToken cancellationToken = default)
	{
		var url = $"api/studios/{id}/games?page={page}";
		if (!string.IsNullOrWhiteSpace(ordering))
			url += $"&ordering={Uri.EscapeDataString(ordering)}";
		var games = await GetAsync<PagedResult<Game>>(url, false, cancellationToken);
		return games ?? new PagedResult<Game>();
	}

	public async Task<List<Game>> GetStudioUpcomingAsync(int id, CancellationToken cancellationToken = default)
	{
		var games = await GetAsync<List<Game>>($"api/studios/{id}/upcoming", false, cancellationToken);
		return games ?? [];
	}

	private async Task SendAsync(HttpMethod method, string url, CancellationToken cancellationToken)
	{
		try
		{
			using var request = new HttpRequestMessage(method, url);
			var response = await _http.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw CreateApiException($"Chiamata {method} a {url} fallita", ex, (ex as HttpRequestException)?.StatusCode);
		}
	}
	public async Task<List<Game>> GetFavoritesAsync(CancellationToken cancellationToken = default)
	{
		var games = await GetAsync<List<Game>>("api/me/favorites", false, cancellationToken);
		return games ?? [];
	}
	public async Task<List<Game>> GetWishlistAsync(CancellationToken cancellationToken = default)
	{
		var games = await GetAsync<List<Game>>("api/me/wishlist", false, cancellationToken);
		return games ?? [];
	}
	public Task AddFavoriteAsync(int id, CancellationToken cancellationToken = default) =>
		SendAsync(HttpMethod.Put, $"api/me/favorites/{id}", cancellationToken);
	public Task RemoveFavoriteAsync(int id, CancellationToken cancellationToken = default) =>
		SendAsync(HttpMethod.Delete, $"api/me/favorites/{id}", cancellationToken);
	public Task AddToWishlistAsync(int id, CancellationToken cancellationToken = default) =>
		SendAsync(HttpMethod.Put, $"api/me/wishlist/{id}", cancellationToken);
	public Task RemoveFromWishlistAsync(int id, CancellationToken cancellationToken = default) =>
		SendAsync(HttpMethod.Delete, $"api/me/wishlist/{id}", cancellationToken);
	// Unico punto in cui nasce un'ApiException con il codice HTTP.
	// 401 = sessione scaduta: si rilegge il profilo, così la navbar torna da utente non entrato
	private ApiException CreateApiException(string message, Exception? inner, HttpStatusCode? status)
	{
		if (status == HttpStatusCode.Unauthorized)
			_auth.RefreshProfile();
		return new ApiException(message, inner, status);
	}
	// 404 → null solo se notFoundAsNull (gioco o studio inesistente), annullamento chiesto dalla pagina → passa così com'è,
	// qualsiasi altro errore (anche un 404 su una lista) → ApiException
	private async Task<T?> GetAsync<T>(string url, bool notFoundAsNull, CancellationToken cancellationToken)
	{
		try
		{
			return await _http.GetFromJsonAsync<T>(url, cancellationToken);
		}
		catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound && notFoundAsNull)
		{
			return default;
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw CreateApiException($"Chiamata a {url} fallita", ex, (ex as HttpRequestException)?.StatusCode);
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

	public async Task<ProfileResponse> GetProfileAsync(CancellationToken cancellationToken = default)
	{
		return await GetAsync<ProfileResponse>("api/me/profile", true, cancellationToken) ?? throw new ApiException("Profilo non trovato", null, HttpStatusCode.NotFound);
	}

	public async Task UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
	{
		try
		{
			using var response = await _http.PutAsJsonAsync("api/me/profile", request, cancellationToken);
			if (!response.IsSuccessStatusCode)
				throw CreateApiException("Modifica profilo fallita", null, response.StatusCode);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex) when (ex is not ApiException)
		{
			throw new ApiException("Chiamata PUT a api/me/profile fallita", ex);
		}
	}
}
