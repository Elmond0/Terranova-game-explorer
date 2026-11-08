using System.Text.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace GameExplorer.Api.Services;

public sealed class TwitchTokenProvider( HttpClient http, HybridCache cache, IOptions<IgdbOptions> options )
{
    private async Task<string> RequestNewTokenAsync(CancellationToken ct)
    {
        var opt = options.Value;
        using var query = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = opt.ClientId,
            ["client_secret"] = opt.ClientSecret,
            ["grant_type"] = "client_credentials"
        });
        using var response = await http.PostAsync("token", query, ct);
        response.EnsureSuccessStatusCode();
        
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
            ?? throw new InvalidOperationException("No response from Twitch");
        return body.AccessToken;
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")]
        string AccessToken,
        [property: JsonPropertyName("expires_in")]
        int ExpiresIn,
        [property: JsonPropertyName("token_type")]
        string TokenType);
        
    public ValueTask<string> GetTokenAsync(CancellationToken ct = default) =>
        cache.GetOrCreateAsync(IgdbCache.TokenKey,
            async token => await RequestNewTokenAsync(token),
            IgdbCache.Token, cancellationToken: ct);

    public ValueTask InvalidateAsync(CancellationToken ct = default) =>
        cache.RemoveAsync(IgdbCache.TokenKey, ct);
}
