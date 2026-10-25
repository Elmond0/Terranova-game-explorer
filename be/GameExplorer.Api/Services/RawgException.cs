using System.Net;
// using System.Net.Http.Json;
// using GameExplorer.Api.Services.Rawg;
// using GameExplorer.Shared.Models;

namespace GameExplorer.Api.Services;

public class RawgException : Exception
{
	public HttpStatusCode	StatusCode { get; }

	public RawgException( HttpStatusCode statusCode, string msg, Exception? inner = null ) : base(msg, inner)
	{
		StatusCode = statusCode;
	}
}