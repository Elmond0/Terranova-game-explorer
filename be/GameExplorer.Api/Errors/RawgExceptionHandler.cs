using System.Xml;
using GameExplorer.Api.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GameExplorer.Api.Errors;

public sealed class RawgExceptionHandler : IExceptionHandler
{
	ILogger<RawgExceptionHandler> _logger;
	public RawgExceptionHandler( ILogger<RawgExceptionHandler> logger ) : base() { _logger = logger; }

	public async ValueTask<bool> TryHandleAsync( HttpContext context, Exception e, CancellationToken ct )
	{
		if (e is not RawgException)
			return false;

		_logger.LogWarning(e, "Rawg does not respond");

		context.Response.StatusCode = StatusCodes.Status502BadGateway;
		await context.Response.WriteAsJsonAsync(new ProblemDetails
		{
			Status = StatusCodes.Status502BadGateway,
			Title = "Rawg service not available.",
			Detail = "Rawg is not responding. Try again."
		}, ct);

		return true;
	}
}