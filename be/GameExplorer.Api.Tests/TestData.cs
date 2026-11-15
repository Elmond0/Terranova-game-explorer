using System.Net;
using Xunit;

namespace GameExplorer.Api.Tests;

public abstract class TestData
{
    protected const string ValidBody = """[{"id":3498,"name":"Grand Theft Auto V","slug":"grand-theft-auto-v"}]""";
    
    protected const string EmptyBody = "[]";
    protected const string ErrorBody = """{"error":"error"}""";
    protected const string BrokenBody = "This is not Jason! D:";

    protected static readonly string[] Endpoints =
    [
        "/api/games/3498",
        "/api/games/top-rated",
        "/api/games/recent",
        "/api/games/upcoming",
        "/api/platforms",
        "/api/genres",
    ];

    protected static readonly HttpStatusCode[] ErrorCodes =
    [
        HttpStatusCode.BadRequest,					// 400
        HttpStatusCode.Unauthorized,				// 401
        HttpStatusCode.Forbidden,					// 403
        HttpStatusCode.NotFound,					// 404 (IGDB non lo usa per "gioco inesistente": e' un errore anomalo)
        HttpStatusCode.TooManyRequests,				// 429
        HttpStatusCode.InternalServerError,			// 500
        HttpStatusCode.BadGateway,					// 502
        HttpStatusCode.ServiceUnavailable,			// 503
        HttpStatusCode.GatewayTimeout,				// 504
    ];

    public static IEnumerable<object[]> OnlyEndpoints()
        => Endpoints.Select(e => new object[] { e });

    public static IEnumerable<object[]> EndpointsPerCode()
        =>	from e in Endpoints
            from c in ErrorCodes
            select new object[] { e, c };
}