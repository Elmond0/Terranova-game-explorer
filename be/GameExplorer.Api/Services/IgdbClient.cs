using System.Text.Json;
using System.Net;
using System.Text;
using GameExplorer.Api.Services.IgdbDtos;
using GameExplorer.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http.Json;

namespace GameExplorer.Api.Services;

public class IgdbClient( HttpClient http )
{
    public async Task<List<T>> QueryAsync<T>(string url, string query, CancellationToken ct = default)
    {
        using var content = new StringContent(query, Encoding.UTF8, "text/plain");
        using var response = await http.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<T>>(ct) ?? [];
    }

    public async Task<int> QueryCountAsync(string url, string query, CancellationToken ct = default)
    {
        using var content = new StringContent(query, Encoding.UTF8, "text/plain");
        using var response = await http.PostAsync($"{url}/count", content, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<IgdbCount>(ct);
        return result?.Count ?? 0;
    }
    
    private record IgdbCount(int Count);
}