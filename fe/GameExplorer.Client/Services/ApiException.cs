using System.Net;

namespace GameExplorer.Client.Services;

public class ApiException : Exception
{
	public HttpStatusCode? StatusCode { get; }
	public ApiException(string message , Exception? innerException = null, HttpStatusCode? statusCode = null) : base(message, innerException)
	{
		StatusCode = statusCode;
	}
}
