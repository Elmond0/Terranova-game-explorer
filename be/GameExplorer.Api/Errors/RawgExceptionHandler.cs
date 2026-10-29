using System.Xml;
using GameExplorer.Api.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GameExplorer.Api.Errors;

public sealed class RawgExceptionHandler : IExceptionHandler
{
	ILogger<RawgExceptionHandler> 	_logger;
	IProblemDetailsService 			_problems;
	public RawgExceptionHandler( ILogger<RawgExceptionHandler> logger, IProblemDetailsService problems ) : base() { _logger = logger; _problems = problems; }

	public async ValueTask<bool> TryHandleAsync( HttpContext context, Exception e, CancellationToken ct )
	{
		if (e is not RawgException)
			return false;

		_logger.LogWarning(e, "Rawg does not respond");

		context.Response.StatusCode = StatusCodes.Status502BadGateway;
		return await _problems.TryWriteAsync(new ProblemDetailsContext
		{
			HttpContext = context,
			Exception = e,
			ProblemDetails = new ProblemDetails
			{	
				Status = StatusCodes.Status502BadGateway,
				Title = "Rawg service not available.",
				Detail = "Rawg is not responding. Try again."
			}
		});
	}
}