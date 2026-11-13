using System.Text.Json;
using System.Net;
using System.Text;
using GameExplorer.Api.Services.IgdbDtos;
using Polly;
using GameExplorer.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http.Json;

namespace GameExplorer.Api.Services;

public class IgdbClient( HttpClient http )
{
    public async Task<List<T>> QueryAsync<T>(string body, CancellationToken ct = default)
        where T : IIgdbEntity => 
        await PostAsync<List<T>>(T.Url, body, ct) ?? [];

    public async Task<int> QueryCountAsync<T>(string body, CancellationToken ct = default)
        where T : IIgdbEntity
    {
        var result = await PostAsync<IgdbCount>($"{T.Url}/count", body, ct);
        return result?.Count ?? 0;
    }

    private async Task<T?> PostAsync<T>(string url, string body, CancellationToken ct = default)
    {
        try
        {
            using var content = new StringContent(body, Encoding.UTF8, "text/plain");
            using var response = await http.PostAsync(url, content, ct);
            if (!response.IsSuccessStatusCode)
                throw new IgdbException($"IGDB answered {(int)response.StatusCode} on '{url}'.");
            return await response.Content.ReadFromJsonAsync<T>(ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException e)
        {
            throw new IgdbException($"IGDB timed out on '{url}'.", e);
        }
        catch (ExecutionRejectedException e)
        {
            throw new IgdbException($"IGDB call rejected by the resilience pipeline on '{url}'.", e);
        }
        catch (Exception e) when (e is HttpRequestException or JsonException or NotSupportedException)
        {
            throw new IgdbException($"IGDB call failed on '{url}'.", e);
        }

    }
    
    private record IgdbCount(int Count);
}