using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.RateLimiting;

namespace GameExplorer.Api.Services;

/// <summary>
/// Waits for a permit from the shared IGDB rate limiter before each request goes out.
/// Registered after AddStandardResilienceHandler, so it is the innermost handler
/// and every retry also goes through the limiter.
/// Logs how long each request waited in the queue and how long IGDB took to answer.
/// </summary>
public sealed class IgdbRateLimitHandler(
    [FromKeyedServices(IgdbRateLimitHandler.LimiterKey)] RateLimiter limiter,
    ILogger<IgdbRateLimitHandler> logger) : DelegatingHandler
{
    public const string LimiterKey = "igdb";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var start = Stopwatch.GetTimestamp();

        // Waits in the queue until a token is available (or the request is cancelled).
        using RateLimitLease lease = await limiter.AcquireAsync(1, cancellationToken);
        var queued = Stopwatch.GetElapsedTime(start);

        if (!lease.IsAcquired)
        {
            logger.LogWarning("IGDB {Path}: rejected by the local rate limiter after {Queue} ms (queue full)",
                request.RequestUri?.AbsolutePath,
                (int)queued.TotalMilliseconds);

            // Queue full: answer locally with 429 without hitting IGDB.
            var rejected = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                RequestMessage = request,
            };

            if (lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
            {
                rejected.Headers.RetryAfter = new RetryConditionHeaderValue(retryAfter);
            }

            return rejected;
        }

        var response = await base.SendAsync(request, cancellationToken);
        var total = Stopwatch.GetElapsedTime(start);

        logger.LogInformation("IGDB {Path}: queue {Queue} ms, igdb {Igdb} ms, status {Status}",
            request.RequestUri?.AbsolutePath,
            (int)queued.TotalMilliseconds,
            (int)(total - queued).TotalMilliseconds,
            (int)response.StatusCode);

        return response;
    }
}