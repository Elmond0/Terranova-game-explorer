using System.Security.Claims;
using System.Net.Http.Json;
using  Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using GameExplorer.Shared.Models;

namespace GameExplorer.Client.Services.Auth;

public class CookieAuthenticationStateProvider :  AuthenticationStateProvider
{
	public const string AvatarClaimType = "avatar";
	private readonly HttpClient _http;
	private record RegisterErrors(Dictionary<string, string[]>? Errors);
	private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));
	public CookieAuthenticationStateProvider(IHttpClientFactory account)
	{
		_http = account.CreateClient("Account");
	}
	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		try
		{
			var response = await _http.GetAsync("api/account/manage/info");
			if (!response.IsSuccessStatusCode)
				return Anonymous;
			var info = await response.Content.ReadFromJsonAsync<ProfileResponse>();
			if (info is null)
				return Anonymous;
			List<Claim> claims =
			[
				new (ClaimTypes.Name, info.DisplayName ?? info.Email),
				new (ClaimTypes.Email, info.Email),
			];
			if (info.Avatar is not null)
				claims.Add(new Claim(AvatarClaimType, info.Avatar));
			var identity = new ClaimsIdentity(claims, "Cookies");
			return new AuthenticationState(new ClaimsPrincipal(identity));
		}
		catch (HttpRequestException)
		{
			return Anonymous;
		}
	}
	public async Task<bool> LoginAsync(string email, string password)
	{
		var response = await _http.PostAsJsonAsync("api/account/login?useCookies=true", new { email, password });
		if (response.StatusCode == HttpStatusCode.Unauthorized)
			return false;
		response.EnsureSuccessStatusCode();
		NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		return true;
	}

	public async Task<string[]> RegisterAsync(string email, string password)
	{
		var response = await _http.PostAsJsonAsync("api/account/register", new { email, password });
		if (response.StatusCode == HttpStatusCode.BadRequest)
		{
			var problem = await response.Content.ReadFromJsonAsync<RegisterErrors>();
			return problem?.Errors?.Keys.ToArray() ?? ["Unknown"];
		}
		response.EnsureSuccessStatusCode();
		await LoginAsync(email, password);
		return [];
	}
	public async Task LogoutAsync()
	{
		try
		{
			await _http.PostAsync("api/account/logout", null);
		}
		catch (HttpRequestException)
		{
		}
		NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
	}

	public void RefreshProfile() =>
		NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
