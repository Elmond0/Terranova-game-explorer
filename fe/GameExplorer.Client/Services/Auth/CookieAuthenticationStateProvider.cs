using System.Security.Claims;
using System.Net.Http.Json;
using  Microsoft.AspNetCore.Components.Authorization;
using System.Net;

namespace GameExplorer.Client.Services.Auth;

public class CookieAuthenticationStateProvider :  AuthenticationStateProvider
{
	private readonly HttpClient _http;
	private record UserInfo(string Email);
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
			var info = await response.Content.ReadFromJsonAsync<UserInfo>();
			if (info is null)
				return Anonymous;
			List<Claim> claims =
			[
				new (ClaimTypes.Name, info.Email),
				new (ClaimTypes.Email, info.Email),
			];
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
}
